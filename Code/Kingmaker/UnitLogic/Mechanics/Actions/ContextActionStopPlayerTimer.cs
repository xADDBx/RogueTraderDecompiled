using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Timer;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Serializable]
[TypeId("d5a0f650c7d84a9096858d7fec27aa5d")]
public class ContextActionStopPlayerTimer : ContextAction
{
	public enum TimerAction
	{
		Stop,
		Pause,
		Resume
	}

	[SerializeField]
	[ValidateNotNull]
	private BlueprintPlayerTimer.Reference m_Blueprint;

	[SerializeField]
	private TimerAction m_Action;

	public override string GetCaption()
	{
		return $"{m_Action} player timer {m_Blueprint.NameSafe()}";
	}

	protected override void RunAction()
	{
		if (m_Blueprint == null || m_Blueprint.IsEmpty())
		{
			Element.LogError(this, "No timer blueprint is set!");
			return;
		}
		PlayerTimersManager timers = Game.Instance.Player.Timers;
		switch (m_Action)
		{
		case TimerAction.Stop:
			timers.Stop(m_Blueprint);
			break;
		case TimerAction.Pause:
			timers.SetPaused(m_Blueprint, isPaused: true);
			break;
		case TimerAction.Resume:
			timers.SetPaused(m_Blueprint, isPaused: false);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
