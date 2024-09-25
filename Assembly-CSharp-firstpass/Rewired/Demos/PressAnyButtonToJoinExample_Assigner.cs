using System.Collections.Generic;
using UnityEngine;

namespace Rewired.Demos;

[AddComponentMenu("")]
public class PressAnyButtonToJoinExample_Assigner : MonoBehaviour
{
	private void Update()
	{
		if (ReInput.isReady)
		{
			AssignJoysticksToPlayers();
		}
	}

	private void AssignJoysticksToPlayers()
	{
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int i = 0; i < joysticks.Count; i++)
		{
			Joystick joystick = joysticks[i];
			if (!ReInput.controllers.IsControllerAssigned(joystick.type, joystick.id) && joystick.GetAnyButtonDown())
			{
				Player player = FindPlayerWithoutJoystick();
				if (player == null)
				{
					return;
				}
				player.controllers.AddController(joystick, removeFromOtherPlayers: false);
			}
		}
		if (DoAllPlayersHaveJoysticks())
		{
			ReInput.configuration.autoAssignJoysticks = true;
			base.enabled = false;
		}
	}

	private Player FindPlayerWithoutJoystick()
	{
		IList<Player> players = ReInput.players.Players;
		for (int i = 0; i < players.Count; i++)
		{
			if (players[i].controllers.joystickCount <= 0)
			{
				return players[i];
			}
		}
		return null;
	}

	private bool DoAllPlayersHaveJoysticks()
	{
		return FindPlayerWithoutJoystick() == null;
	}
}
