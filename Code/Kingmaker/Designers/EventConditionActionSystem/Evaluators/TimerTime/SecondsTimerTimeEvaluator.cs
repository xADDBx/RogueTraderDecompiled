using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Timer;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators.TimerTime;

[Serializable]
[TypeId("255aa7a08b8540708796f19fbdc16c85")]
public class SecondsTimerTimeEvaluator : TimerTimeEvaluator
{
	[SerializeField]
	private float m_Time;

	public override string GetCaption()
	{
		return $"Timer after {m_Time} seconds";
	}

	protected override Kingmaker.Controllers.Timer.TimerTime GetValueInternal()
	{
		return new Kingmaker.Controllers.Timer.TimerTime(TimeSpan.FromSeconds(m_Time));
	}
}
