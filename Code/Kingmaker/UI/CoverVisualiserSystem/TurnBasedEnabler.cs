using Kingmaker.Controllers.TurnBased;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.UI.CoverVisualiserSystem;

public class TurnBasedEnabler : MonoBehaviour, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler
{
	public GameObject WhatToEnable;

	private void OnEnable()
	{
		EventBus.Subscribe(this);
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe(this);
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		TurnBasedModeHandle(isTurnBased);
	}

	public void HandleTurnBasedModeResumed()
	{
		TurnBasedModeHandle(isTurnBased: true);
	}

	private void TurnBasedModeHandle(bool isTurnBased)
	{
		if (Game.Instance.CurrentMode != GameModeType.SpaceCombat && (bool)WhatToEnable)
		{
			WhatToEnable.SetActive(isTurnBased);
		}
	}
}
