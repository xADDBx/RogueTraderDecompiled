using Kingmaker.QA.Arbiter.Tasks;

namespace Kingmaker.QA.Arbiter.Service;

public class BlueprintArbiterInstructionArbiterTaskFactory : IArbiterTaskFactory
{
	public ArbiterTask Create(InstructionInfo instruction, ArbiterStartupParameters parameters)
	{
		BlueprintArbiterInstruction instruction2 = BlueprintArbiterInstructionIndex.Instance.GetInstruction(instruction.Name);
		if (instruction2 == null)
		{
			throw new ArbiterException("Blueprint " + instruction.Name + " is not found.");
		}
		return ((instruction2.Test as IArbiterCheckerComponent) ?? throw new ArbiterException($"Instruction '{instruction.Name}' is unknown test type {instruction2.Test.GetType()}")).GetArbiterTask(parameters);
	}
}
