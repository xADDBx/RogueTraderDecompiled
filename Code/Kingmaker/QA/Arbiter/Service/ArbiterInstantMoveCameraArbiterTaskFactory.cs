using Kingmaker.QA.Arbiter.Tasks;

namespace Kingmaker.QA.Arbiter.Service;

public class ArbiterInstantMoveCameraArbiterTaskFactory : IArbiterTaskFactory
{
	public ArbiterTask Create(InstructionInfo instruction, ArbiterStartupParameters parameters)
	{
		if (instruction.Name.Contains("instant_move_camera"))
		{
			return new ArbiterInstantMoveCameraTask(instruction.Name);
		}
		return null;
	}
}
