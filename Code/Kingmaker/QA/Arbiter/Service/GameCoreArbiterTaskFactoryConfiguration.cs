namespace Kingmaker.QA.Arbiter.Service;

public class GameCoreArbiterTaskFactoryConfiguration : ArbiterTaskFactoryConfiguration
{
	public GameCoreArbiterTaskFactoryConfiguration()
	{
		base.TaskFactories.Add(new ExportMapsArbiterTaskFactory());
		base.TaskFactories.Add(new ArbiterInstantMoveCameraArbiterTaskFactory());
		base.TaskFactories.Add(new BlueprintArbiterInstructionArbiterTaskFactory());
	}
}
