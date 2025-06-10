using Kingmaker.QA.Arbiter.Service;
using Kingmaker.QA.Arbiter.Service.Interfaces;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.QA.Arbiter;

internal class BlueprintArbiterInstructionIndex : BotInstructionIndex<BlueprintArbiterInstruction>, IArbiterInstructionIndex
{
	private static BlueprintArbiterInstructionIndex s_Instance;

	public static BlueprintArbiterInstructionIndex Instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = new BlueprintArbiterInstructionIndex();
			}
			return s_Instance;
		}
	}

	protected override LogChannel GetLogChannel()
	{
		return ArbiterService.Logger;
	}
}
