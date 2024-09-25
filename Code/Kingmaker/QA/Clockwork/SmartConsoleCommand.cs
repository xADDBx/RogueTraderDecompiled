using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.QA.Clockwork;

[ComponentName("ClockworkRules/SmartConsoleCommand")]
[TypeId("aeca337839ed5334caa7fcce75309114")]
public class SmartConsoleCommand : ClockworkCommand
{
	public string LineToExecute;

	public override ClockworkRunnerTask GetTask(ClockworkRunner runner)
	{
		Complete();
		SmartConsole.ExecuteLine(LineToExecute);
		return null;
	}

	public override string GetCaption()
	{
		return GetStatusString() + "SmartConsole execute line: " + LineToExecute;
	}
}
