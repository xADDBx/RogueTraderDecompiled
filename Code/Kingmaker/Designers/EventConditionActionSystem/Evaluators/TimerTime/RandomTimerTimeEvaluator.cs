using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Timer;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators.TimerTime;

[Serializable]
[TypeId("5d2d68c93eda48f6bae1bba14270dfc9")]
public class RandomTimerTimeEvaluator : TimerTimeEvaluator
{
	[SerializeField]
	private float m_MinTime;

	[SerializeField]
	private float m_MaxTime;

	public override string GetCaption()
	{
		return $"Timer after ({m_MinTime}, {m_MaxTime}) seconds";
	}

	protected override Kingmaker.Controllers.Timer.TimerTime GetValueInternal()
	{
		return new Kingmaker.Controllers.Timer.TimerTime(TimeSpan.FromSeconds(PFStatefulRandom.Timer.Range(m_MinTime, m_MaxTime)));
	}
}
