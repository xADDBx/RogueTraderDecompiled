using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Owlcat.Runtime.Visual.RenderPipeline.Lighting;

[BurstCompile]
public struct ReorderJob<T> : IJobFor where T : struct
{
	[ReadOnly]
	public NativeArray<int> Indices;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<T> Input;

	public NativeArray<T> Output;

	public void Execute(int index)
	{
		int index2 = Indices[index];
		Output[index] = Input[index2];
	}
}
