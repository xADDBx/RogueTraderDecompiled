using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.QA.Clockwork;

[ComponentName("ClockworkRules/EndScenarioCommand")]
[TypeId("9982bdd07cea65743a464a082643d2ab")]
public class EndScenarioCommand : ClockworkCommand
{
	public override ClockworkRunnerTask GetTask(ClockworkRunner runner)
	{
		runner.Paused = true;
		PFLog.Clockwork.Log("Clockwork Scenario End!");
		if (!Clockwork.Instance.ShowDebugEndMessage)
		{
			Clockwork.Instance.Stop();
		}
		return null;
	}

	public override string GetCaption()
	{
		return "End scenario";
	}
}
