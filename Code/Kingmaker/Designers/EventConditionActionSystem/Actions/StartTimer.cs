using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Timer;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Serializable]
[ComponentName("Actions/StartTimer")]
[AllowMultipleComponents]
[TypeId("c3f192c81bae4448865ae282bc208bb8")]
public class StartTimer : GameAction
{
	[SerializeReference]
	public TimerTimeEvaluator TimerTime;

	[SerializeField]
	private ActionList m_Actions;

	[SerializeField]
	private ConditionsChecker m_Conditions;

	public override string GetCaption()
	{
		return "Start timer";
	}

	public override void RunAction()
	{
		if (m_Conditions.Check())
		{
			SimpleTimer timer = new SimpleTimer(m_Actions.Run, TimerTime.GetValue().Time);
			EventBus.RaiseEvent(delegate(ITimerHandler e)
			{
				e.SubscribeTimer(timer);
			});
		}
	}
}
