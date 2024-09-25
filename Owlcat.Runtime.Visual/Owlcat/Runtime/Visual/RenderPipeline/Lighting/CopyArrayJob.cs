using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Owlcat.Runtime.Visual.RenderPipeline.Lighting;

[BurstCompile]
public struct CopyArrayJob<T> : IJob where T : struct
{
	[ReadOnly]
	public int Count;

	[ReadOnly]
	public NativeArray<T> Input;

	[WriteOnly]
	public NativeArray<T> Output;

	public void Execute()
	{
		NativeArray<T>.Copy(Input, Output, Count);
	}
}
