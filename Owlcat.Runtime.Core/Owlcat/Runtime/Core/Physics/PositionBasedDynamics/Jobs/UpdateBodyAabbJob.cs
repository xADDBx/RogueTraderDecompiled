using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Jobs;

[BurstCompile]
public struct UpdateBodyAabbJob : IJobParallelFor
{
	public int BodiesAabbOffset;

	[ReadOnly]
	public NativeArray<int> BodyDescriptorsIndices;

	[ReadOnly]
	public NativeArray<int2> ParticlesOffsetCount;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<ParticlePositionPair> PositionPairs;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<ParticleExtendedData> ExtendedData;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> AabbMin;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> AabbMax;

	public void Execute(int index)
	{
		int index2 = BodyDescriptorsIndices[index];
		int2 @int = ParticlesOffsetCount[index2];
		float3 position = PositionPairs[@int.x].Position;
		float3 @float = PositionPairs[@int.x].BasePosition;
		float3 float2 = position - @float;
		position = ((math.dot(float2, float2) < 400f) ? position : @float);
		float3 float3 = position - ExtendedData[@int.x].Radius;
		float3 float4 = position + ExtendedData[@int.x].Radius;
		for (int i = 1; i < @int.y; i++)
		{
			int index3 = i + @int.x;
			position = PositionPairs[index3].Position;
			@float = @float[index3];
			float3 float5 = position - @float;
			position = ((math.dot(float5, float5) < 400f) ? position : @float);
			float radius = ExtendedData[index3].Radius;
			float3 = math.min(float3, position - radius);
			float4 = math.max(float4, position + radius);
		}
		AabbMin[index + BodiesAabbOffset] = float3;
		AabbMax[index + BodiesAabbOffset] = float4;
	}
}
