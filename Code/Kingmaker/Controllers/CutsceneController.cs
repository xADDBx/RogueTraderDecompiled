using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Code.UI.MVVM.VM.Fade;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Controllers;

public class CutsceneController : IControllerEnable, IController, IControllerDisable, IControllerTick, IGameModeHandler, ISubscriber
{
	private static readonly LogChannel Logger = PFLog.Cutscene;

	private readonly bool m_TickBackground;

	private static bool s_ShouldStartSkipping;

	public static bool Skipping { get; private set; }

	public static CountableFlag LockSkipBarkBanter { get; } = new CountableFlag();


	public CutsceneController(bool tickBackground)
	{
		m_TickBackground = tickBackground;
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.StarSystem && Game.Instance.CurrentMode != GameModeType.CutsceneGlobalMap)
		{
			return;
		}
		CutsceneLock.CheckRequest();
		if (s_ShouldStartSkipping && LoadingProcess.Instance.IsLoadingScreenShown)
		{
			StartCutsceneSkip();
		}
		foreach (CutscenePlayerData cutscene in Game.Instance.State.Cutscenes)
		{
			using (ProfileScope.New("Tick Scene"))
			{
				cutscene.TickScene(Skipping && cutscene.Cutscene.LockControl && !cutscene.Cutscene.NonSkippable);
			}
		}
		CutsceneLock.CheckRelease();
	}

	public void OnEnable()
	{
		if (Game.Instance.CurrentMode == GameModeType.Cutscene)
		{
			foreach (UnitGroup unitGroup in Game.Instance.UnitGroups)
			{
				if (!unitGroup.IsInCombat)
				{
					continue;
				}
				for (int i = 0; i < unitGroup.Count; i++)
				{
					BaseUnitEntity baseUnitEntity = unitGroup[i];
					if (baseUnitEntity != null && baseUnitEntity.IsInCombat && baseUnitEntity.Brain != null && !baseUnitEntity.Passive)
					{
						baseUnitEntity.Commands.InterruptAllInterruptible();
					}
				}
			}
		}
		if (m_TickBackground)
		{
			return;
		}
		foreach (CutscenePlayerData cutscene in Game.Instance.State.Cutscenes)
		{
			if (cutscene.Cutscene.IsBackground)
			{
				cutscene.SetPaused(value: true, CutscenePauseReason.GameModePauseBackgroundCutscenes);
			}
		}
	}

	public void OnDisable()
	{
		if (m_TickBackground)
		{
			return;
		}
		foreach (CutscenePlayerData cutscene in Game.Instance.State.Cutscenes)
		{
			if (cutscene.Cutscene.IsBackground)
			{
				cutscene.SetPaused(value: false, CutscenePauseReason.GameModePauseBackgroundCutscenes);
			}
		}
	}

	public static void SkipCutsceneInternal()
	{
		if (!Skipping && !s_ShouldStartSkipping && (!LoadingProcess.Instance.IsLoadingScreenActive || LoadingProcess.Instance.IsFadeActive) && (Game.Instance.CurrentMode == GameModeType.Cutscene || Game.Instance.CurrentMode == GameModeType.CutsceneGlobalMap))
		{
			if (Game.Instance.State.Cutscenes.TryFind((CutscenePlayerData p) => p.Cutscene.LockControl && p.Cutscene.NonSkippable, out var result))
			{
				Logger.Log($"Can't skip non-skippable cutscene {result}");
				return;
			}
			Logger.Log("Start skipping cutscene");
			SoundState.Instance?.StartCutsceneSkip();
			FadeCanvas.Instance?.ShowLoadingScreen();
			StartCutsceneSkip();
		}
	}

	public static bool SkipBarkBanter()
	{
		if ((bool)LockSkipBarkBanter)
		{
			return false;
		}
		if (Game.Instance.CurrentMode == GameModeType.Cutscene || Game.Instance.CurrentMode == GameModeType.CutsceneGlobalMap)
		{
			List<CutscenePlayerData> list = Game.Instance.State.Cutscenes.Where((CutscenePlayerData p) => p.Cutscene.LockControl).ToTempList();
			foreach (CutscenePlayerData item in list)
			{
				item.InterruptBark();
			}
			return list.Count > 0;
		}
		return false;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if ((gameMode == GameModeType.Cutscene || gameMode == GameModeType.CutsceneGlobalMap) && (Skipping || s_ShouldStartSkipping))
		{
			Logger.Log("Stop skipping cutscene");
			Skipping = false;
			s_ShouldStartSkipping = false;
			Game.Instance.TimeController.DebugTimeScale = 1f;
			SoundState.Instance.StopCutsceneSkip();
			FadeCanvas.Instance.HideLoadingScreen();
			FadeCanvas.Fadeout(fade: false);
		}
	}

	private static void StartCutsceneSkip()
	{
		s_ShouldStartSkipping = false;
		Skipping = true;
	}
}
