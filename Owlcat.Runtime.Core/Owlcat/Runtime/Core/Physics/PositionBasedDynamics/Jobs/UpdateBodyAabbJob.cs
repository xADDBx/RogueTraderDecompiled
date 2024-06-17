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
	public NativeArray<float3> Position;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> BasePosition;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float> Radius;

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
		float3 @float = Position[@int.x];
		float3 float2 = BasePosition[@int.x];
		float3 float3 = @float - float2;
		@float = ((math.dot(float3, float3) < 400f) ? @float : float2);
		float3 float4 = @float - Radius[@int.x];
		float3 float5 = @float + Radius[@int.x];
		for (int i = 1; i < @int.y; i++)
		{
			int index3 = i + @int.x;
			@float = Position[index3];
			float2 = float2[index3];
			float3 float6 = @float - float2;
			@float = ((math.dot(float6, float6) < 400f) ? @float : float2);
			float num = Radius[index3];
			float4 = math.min(float4, @float - num);
			float5 = math.max(float5, @float + num);
		}
		AabbMin[index + BodiesAabbOffset] = float4;
		AabbMax[index + BodiesAabbOffset] = float5;
	}
}
