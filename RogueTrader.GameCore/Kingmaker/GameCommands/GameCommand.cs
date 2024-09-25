using System;
using CodeGenerators.MemoryPackUnionGenerator;
using MemoryPack;

namespace Kingmaker.GameCommands;

[MemoryPackable(GenerateType.NoGenerate)]
[MemoryPackDynamicUnion]
public abstract class GameCommand
{
	[MemoryPackIgnore]
	public virtual bool IsSynchronized => false;

	[MemoryPackIgnore]
	public virtual bool IsForcedSynced => false;

	public void Execute()
	{
		try
		{
			ExecuteInternal();
		}
		catch (Exception ex)
		{
			PFLog.System.Exception(ex, "Failed to execute game command " + GetType().Name);
		}
	}

	protected abstract void ExecuteInternal();

	public virtual void AfterDeserialization()
	{
	}
}
