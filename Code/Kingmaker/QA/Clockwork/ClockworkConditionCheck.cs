using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.QA.Clockwork;

[TypeId("6c24cdbf84df16c4c81f666444959f25")]
public class ClockworkConditionCheck : ClockworkCheck
{
	[SerializeReference]
	public Condition Check;

	public override ClockworkRunnerTask GetTask(ClockworkRunner runner)
	{
		return new TaskDelayedCall(runner, delegate
		{
			base.LastResult = Check.Check();
			Clockwork.Instance.Reporter.NeedReport = true;
			Complete();
			PFLog.Clockwork.Log($"[{base.LastResult}] {GetCaption()}");
		}, GetCaption(), 1f);
	}

	public override string GetCaption()
	{
		return GetStatusString() + "Check: <" + Check.GetCaption() + ">";
	}
}
