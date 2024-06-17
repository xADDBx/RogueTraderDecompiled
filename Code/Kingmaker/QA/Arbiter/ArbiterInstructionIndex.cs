using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.QA.Arbiter;

public class ArbiterInstructionIndex : BotInstructionIndex<BlueprintArbiterInstruction>
{
	private static ArbiterInstructionIndex s_Instance;

	public static ArbiterInstructionIndex Instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = new ArbiterInstructionIndex();
			}
			return s_Instance;
		}
	}

	public ArbiterInstructionIndex()
		: base("arbiter.json")
	{
	}

	protected override LogChannel GetLogChannel()
	{
		return PFLog.Arbiter;
	}
}
