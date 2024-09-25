using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Replay;
using Kingmaker.Settings;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View.MapObjects.Traps;
using UnityEngine;

namespace Kingmaker.Controllers;

public class AutoPauseController : IController, IAwarenessHandler, ISubscriber<IMapObjectEntity>, ISubscriber, IFocusHandler, IAreaHandler
{
	private static void Pause(bool condition = true, BaseUnitEntity unit = null)
	{
		if (!TurnController.IsInTurnBasedCombat() && condition && (unit == null || unit.IsDirectlyControllable))
		{
			Game.Instance.IsPaused = true;
		}
	}

	public void OnEntityNoticed(BaseUnitEntity character)
	{
		MapObjectEntity entity = EventInvokerExtensions.GetEntity<MapObjectEntity>();
		if (!TurnController.IsInTurnBasedCombat())
		{
			Pause((entity is TrapObjectData && (bool)SettingsRoot.Game.Autopause.PauseOnTrapDetected) || (bool)SettingsRoot.Game.Autopause.PauseOnHiddenObjectDetected, character);
			return;
		}
		if (TurnController.IsInTurnBasedCombat() && character.IsInCombat)
		{
			character.Commands.InterruptAllInterruptible();
		}
		if (character.Commands.Empty)
		{
			character.View.StopMoving();
		}
	}

	public void OnApplicationFocusChanged(bool isFocused)
	{
		if (!NetworkingManager.IsActive && !Kingmaker.Replay.Replay.IsActive && !ApplicationFocusEvents.AutoPauseDisabled && (SettingsRoot.Graphics.FullScreenMode.GetValue() == FullScreenMode.ExclusiveFullScreen || (bool)SettingsRoot.Game.Autopause.PauseOnLostFocus))
		{
			Pause(!isFocused);
		}
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		if ((bool)SettingsRoot.Game.Autopause.PauseOnAreaLoaded)
		{
			Pause();
		}
	}
}
