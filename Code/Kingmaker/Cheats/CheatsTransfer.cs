using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Cheats;
using Kingmaker.Blueprints.Area;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.VM.MainMenu;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.InputSystems;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.StateContext;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Cheats;

internal static class CheatsTransfer
{
	public class TeleportAfterAreaLoad : IAreaHandler, ISubscriber
	{
		private readonly float m_X;

		private readonly float m_Y;

		private readonly float m_Z;

		private readonly float m_CameraX;

		private readonly float m_CameraY;

		private readonly float m_CameraZ;

		public static void Schedule(float x, float y, float z, float cameraX, float cameraY, float cameraZ)
		{
			EventBus.Subscribe(new TeleportAfterAreaLoad(x, y, z, cameraX, cameraY, cameraZ));
		}

		internal TeleportAfterAreaLoad()
		{
		}

		private TeleportAfterAreaLoad(float x, float y, float z, float cameraX, float cameraY, float cameraZ)
		{
			m_X = x;
			m_Y = y;
			m_Z = z;
			m_CameraX = cameraX;
			m_CameraY = cameraY;
			m_CameraZ = cameraZ;
		}

		public void OnAreaBeginUnloading()
		{
		}

		public void OnAreaDidLoad()
		{
			LocalTeleport(m_X, m_Y, m_Z, m_CameraX, m_CameraY, m_CameraZ);
			EventBus.Unsubscribe(this);
		}
	}

	public static void RegisterCommands(KeyboardAccess keyboard)
	{
		if (BuildModeUtility.CheatsEnabled)
		{
			keyboard.Bind("LocalTeleport", delegate
			{
				CheatsHelper.Run("local_teleport @cursor @selectedUnits");
			});
			SmartConsole.RegisterCommand("teleport", Teleport);
		}
		if (PlayerPrefs.GetInt("locs_alias", 0) != 1)
		{
			return;
		}
		foreach (string blueprintName in Utilities.GetBlueprintNames<BlueprintAreaEnterPoint>())
		{
			SmartConsole.RegisterCommand("tp2loc_" + blueprintName, delegate
			{
				Tp2Loc(Utilities.GetBlueprint<BlueprintAreaEnterPoint>(blueprintName));
			});
		}
	}

	[Cheat(Name = "tp2loc", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void Tp2Loc(BlueprintAreaEnterPoint enterPoint)
	{
		Game.Instance.LoadArea(enterPoint, AutoSaveMode.None);
	}

	[Cheat(Name = "list_locs", ExecutionPolicy = ExecutionPolicy.All)]
	public static void ListLocs(string nameSubstring)
	{
		foreach (BlueprintAreaEnterPoint scriptableObject in Utilities.GetScriptableObjects<BlueprintAreaEnterPoint>())
		{
			if (string.IsNullOrEmpty(nameSubstring) || scriptableObject.name.Contains(nameSubstring, StringComparison.InvariantCultureIgnoreCase))
			{
				PFLog.SmartConsole.Log(scriptableObject.name + " LocalizedName: " + scriptableObject.Area.AreaName);
			}
		}
	}

	[Cheat(Name = "locs_alias", Description = "Enable/disable commands tp2loc_*location*", ExampleArgs = "True, true, false")]
	public static string EnableLocsAlias(string arg)
	{
		arg = arg.ToLower();
		if (arg == "true")
		{
			PlayerPrefs.SetInt("locs_alias", 1);
			return "locs_alias enabled";
		}
		if (arg == "false")
		{
			PlayerPrefs.SetInt("locs_alias", 0);
			return "locs_alias disabled";
		}
		return "Use true/false";
	}

	[Cheat(Name = "local_teleport", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void LocalTeleport(Vector3 tpPosition, string selectedUnits)
	{
		LocalTeleport(tpPosition, from x in Utilities.GetArguments(selectedUnits)
			select new UnitReference(x).Entity.ToBaseUnitEntity() into x
			where x != null
			select x);
	}

	public static void LocalTeleport(Vector3 tpPosition)
	{
		LocalTeleport(tpPosition, Game.Instance.SelectionCharacter.SelectedUnits);
	}

	public static void LocalTeleport(Vector3 tpPosition, IEnumerable<BaseUnitEntity> units)
	{
		using (ContextData<EditStateContext>.Request())
		{
			foreach (BaseUnitEntity unit in units)
			{
				unit.Commands.InterruptAllInterruptible();
				Vector3 position = (TurnController.IsInTurnBasedCombat() ? (tpPosition.GetNearestNodeXZ()?.Vector3Position ?? tpPosition) : tpPosition);
				unit.Position = position;
				unit.MovementAgent.Blocker.BlockAtCurrentPosition();
			}
		}
	}

	public static void Teleport(string parameters)
	{
		BlueprintArea blueprint = Utilities.GetBlueprint<BlueprintArea>(Utilities.GetParamString(parameters, 1, "Can't parse BlueprintArea"));
		if (blueprint == null)
		{
			return;
		}
		int num = 1;
		float? paramFloat = Utilities.GetParamFloat(parameters, num + 1, "Can't parse parameter x");
		float? paramFloat2 = Utilities.GetParamFloat(parameters, num + 2, "Can't parse parameter y");
		float? paramFloat3 = Utilities.GetParamFloat(parameters, num + 3, "Can't parse parameter z");
		if (!paramFloat.HasValue || !paramFloat2.HasValue || !paramFloat3.HasValue)
		{
			return;
		}
		float? paramFloat4 = Utilities.GetParamFloat(parameters, num + 4, "Can't parse parameter cameraX");
		float? paramFloat5 = Utilities.GetParamFloat(parameters, num + 5, "Can't parse parameter cameraY");
		float? paramFloat6 = Utilities.GetParamFloat(parameters, num + 5, "Can't parse parameter cameraZ");
		if (!paramFloat4.HasValue || !paramFloat5.HasValue || !paramFloat6.HasValue)
		{
			return;
		}
		if (Game.Instance.CurrentlyLoadedArea != blueprint)
		{
			BlueprintAreaEnterPoint enterPoint = Utilities.GetEnterPoint(blueprint);
			if (enterPoint != null)
			{
				Game.Instance.LoadArea(enterPoint, AutoSaveMode.BeforeExit);
				TeleportAfterAreaLoad.Schedule(paramFloat.Value, paramFloat2.Value, paramFloat3.Value, paramFloat4.Value, paramFloat5.Value, paramFloat6.Value);
			}
		}
		else
		{
			LocalTeleport(paramFloat.Value, paramFloat2.Value, paramFloat3.Value, paramFloat4.Value, paramFloat5.Value, paramFloat6.Value);
		}
	}

	[Cheat(Name = "get_position", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static string GetPosition()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			stringBuilder.AppendFormat("{0} position: {1}", partyAndPet.Blueprint.CharacterName, partyAndPet.Position).AppendLine();
		}
		stringBuilder.AppendFormat("Camera position {0}", CameraRig.Instance.GetTargetPointPosition());
		return stringBuilder.ToString();
	}

	private static void LocalTeleport(float x, float y, float z, float cameraX, float cameraY, float cameraZ)
	{
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			partyAndPet.Commands.InterruptAllInterruptible();
			partyAndPet.Position = new Vector3(x, y, z);
			partyAndPet.SnapToGrid();
		}
		CameraRig.Instance.ScrollToImmediately(new Vector3(cameraX, cameraY, cameraZ));
	}

	[Cheat(Name = "list_game_presets", ExecutionPolicy = ExecutionPolicy.All)]
	public static void ListGamePresets()
	{
		foreach (string item in from p in Utilities.GetBlueprintNames<BlueprintAreaPreset>()
			orderby p
			select p)
		{
			PFLog.SmartConsole.Log(item);
		}
	}

	[Cheat(Name = "new_game", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void StartNewGame(BlueprintAreaPreset preset)
	{
		LoadingProcess.Instance.StartCoroutine(NewGameCoroutine(preset));
	}

	private static IEnumerator NewGameCoroutine(BlueprintAreaPreset preset)
	{
		if (Game.Instance.CurrentlyLoadedArea != null)
		{
			RootUIContext context = Game.Instance.RootUiContext;
			Game.Instance.ResetToMainMenu();
			while (LoadingProcess.Instance.IsLoadingInProcess)
			{
				yield return null;
			}
			yield return null;
			context.Dispose();
		}
		if (MainMenuUI.Instance != null)
		{
			MainMenuUI.Instance.EnterGame(delegate
			{
				Game.Instance.LoadNewGame(preset);
			});
		}
	}
}
