using System.Collections;

namespace Kingmaker.QA.Clockwork;

internal class TaskDummy : ClockworkRunnerTask
{
	public TaskDummy(ClockworkRunner runner)
		: base(runner)
	{
		PFLog.Clockwork.Log("TaskDummy ctor");
	}

	protected override IEnumerator Routine()
	{
		yield return 15f;
	}

	public override string ToString()
	{
		return "Do not know what to do...";
	}
}
