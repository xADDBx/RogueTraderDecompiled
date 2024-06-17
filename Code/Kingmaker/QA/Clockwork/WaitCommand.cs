using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Owlcat.QA.Validation;

namespace Kingmaker.QA.Clockwork;

[ComponentName("ClockworkRules/WaitCommand")]
[TypeId("de4aa3fb76a74402928d522e6cde823f")]
public class WaitCommand : ClockworkCommand
{
	[ValidateNotNull]
	public float WaitTime;

	public override ClockworkRunnerTask GetTask(ClockworkRunner runner)
	{
		TaskDelayedCall taskDelayedCall = new TaskDelayedCall(runner, null, GetCaption(), WaitTime * Game.Instance.TimeController.DebugTimeScale);
		taskDelayedCall.SetSourceCommand(this);
		return taskDelayedCall;
	}

	public override string GetCaption()
	{
		return $"{GetStatusString()}Waiting {WaitTime} seconds";
	}
}
