using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Cheats;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.QA.Arbiter.GameCore.AreaChecker;
using Kingmaker.QA.Arbiter.Service;
using Kingmaker.QA.Arbiter.Service.Interfaces;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.Effects.WeatherSystem;
using UnityEngine;

namespace Kingmaker.QA.Arbiter.GameCore;

public class GameCoreArbiterIntegration : IArbiterIntegration
{
	public void TeleportToLocalPoint(Vector3 vector3)
	{
		CheatsTransfer.LocalTeleport(vector3);
	}

	public void MoveCameraToPoint(Vector3 position, float rotation, float zoom)
	{
		CameraRig instance = CameraRig.Instance;
		instance.ScrollToImmediately(position);
		instance.RotateToImmediately(rotation);
		instance.CameraZoom.ZoomToImmediate(zoom);
	}

	public void LockInput()
	{
		ArbiterService.Logger.Warning("LockInput not implemented");
	}

	public void UnLockInput()
	{
		ArbiterService.Logger.Warning("UnLockInput not implemented");
	}

	public bool IsInDefaultMode()
	{
		if (Game.Instance.CurrentMode == GameModeType.Default)
		{
			return !Game.Instance.TurnController.InCombat;
		}
		return false;
	}

	public void GoToDefaultMode()
	{
		if (IsInDefaultMode())
		{
			return;
		}
		if (Game.Instance.CurrentMode == GameModeType.Dialog)
		{
			Game.Instance.DialogController.StopDialog();
		}
		else if (Game.Instance.CurrentMode == GameModeType.Cutscene)
		{
			ArbiterService.Logger.Log("Force stop locked cutscenes");
			foreach (CutscenePlayerData item in Game.Instance.State.Cutscenes.ToTempList())
			{
				if (item.Cutscene.LockControl)
				{
					item.Stop();
				}
			}
			Game.Instance.GameCommandQueue.SkipCutscene();
		}
		else if (Game.Instance.CurrentMode == GameModeType.Pause)
		{
			ArbiterIntegration.SetGamePause(value: false);
		}
		else if (Game.Instance.TurnController.InCombat)
		{
			Game.Instance.TurnController.OnStart();
			CheatsCombat.KillAll();
		}
		else
		{
			ArbiterService.Logger.Error("Unawaitable game mode {0}", Game.Instance.CurrentMode);
		}
	}

	public RandomFactorsState ExcludeRandomFactors()
	{
		RandomFactorsState randomFactorsState = new RandomFactorsState();
		InclemencyType inclemencyType = ArbiterIntegration.SetWeather(InclemencyType.Clear);
		randomFactorsState.Add("oldWeather", inclemencyType);
		ArbiterIntegration.HideUi();
		ArbiterIntegration.DisableClouds();
		ArbiterIntegration.DisableWind();
		ArbiterIntegration.DisableFog();
		ArbiterIntegration.DisableFow();
		ArbiterIntegration.DisableFx();
		ArbiterIntegration.HideUnits();
		return randomFactorsState;
	}

	public void IncludeRandomFactors(RandomFactorsState oldFactors)
	{
		ArbiterIntegration.ShowUnits();
		ArbiterIntegration.EnableFx();
		ArbiterIntegration.EnableFow();
		ArbiterIntegration.EnableFog();
		ArbiterIntegration.EnableWind();
		ArbiterIntegration.EnableClouds();
		if (oldFactors.Get<InclemencyType>("oldWeather", out var value))
		{
			ArbiterIntegration.SetWeather(value);
		}
		ArbiterIntegration.ShowUi();
	}

	public string GetCurrentlyLoadedAreaName()
	{
		return Game.Instance.CurrentlyLoadedArea.AreaName.Text;
	}

	public void SetGamePause(bool state)
	{
		ArbiterIntegration.SetGamePause(state);
	}

	public void TakeScreenshot(string probeDataDataFolder, int pointSampleId)
	{
		ArbiterIntegration.TakeScreenshot(probeDataDataFolder, pointSampleId);
	}
}
