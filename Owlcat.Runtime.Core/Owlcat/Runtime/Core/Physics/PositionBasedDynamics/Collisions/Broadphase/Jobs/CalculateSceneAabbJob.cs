using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase.Jobs;

[BurstCompile]
public struct CalculateSceneAabbJob : IJob
{
	[ReadOnly]
	public int Count;

	[ReadOnly]
	public NativeArray<float3> AabbMin;

	[ReadOnly]
	public NativeArray<float3> AabbMax;

	[WriteOnly]
	public NativeArray<float3> SceneAabb;

	public void Execute()
	{
		float3 @float = AabbMin[0];
		float3 float2 = AabbMax[0];
		for (int i = 1; i < Count; i++)
		{
			@float = math.min(@float, AabbMin[i]);
			float2 = math.max(float2, AabbMax[i]);
		}
		SceneAabb[0] = @float;
		SceneAabb[1] = float2;
	}
}
