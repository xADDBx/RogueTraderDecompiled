using CodeGenerators.MemoryPackUnionGenerator;
using MemoryPack;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.NoGenerate)]
[MemoryPackDynamicUnion]
public abstract class GameCommandWithSynchronized : GameCommand
{
	protected bool m_IsSynchronized;

	public override bool IsSynchronized => m_IsSynchronized;

	public override void AfterDeserialization()
	{
		base.AfterDeserialization();
		m_IsSynchronized = true;
	}
}
