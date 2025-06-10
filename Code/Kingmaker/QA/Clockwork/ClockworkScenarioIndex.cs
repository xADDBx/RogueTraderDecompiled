using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.QA.Clockwork;

public class ClockworkScenarioIndex : BotInstructionIndex<BlueprintClockworkScenario>
{
	private static ClockworkScenarioIndex s_Instance;

	public static ClockworkScenarioIndex Instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = new ClockworkScenarioIndex();
			}
			return s_Instance;
		}
	}

	protected override LogChannel GetLogChannel()
	{
		return PFLog.Clockwork;
	}
}
