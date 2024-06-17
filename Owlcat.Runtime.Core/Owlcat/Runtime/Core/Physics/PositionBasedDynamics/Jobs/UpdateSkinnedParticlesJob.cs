using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Jobs;

[BurstCompile]
public struct UpdateSkinnedParticlesJob : IJobParallelFor
{
	[ReadOnly]
	public int Offset;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int2> ParticlesOffsetCount;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> BonesOffset;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> BonesCount;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float> TeleportDistanceTreshold;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> SkinnedBodyIndices;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4x4> LocalToWorldMatrices;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float> Mass;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<Matrix4x4> Boneposes;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> Position;

	[NativeDisableParallelForRestriction]
	public NativeArray<float3> BasePosition;

	public void Execute(int index)
	{
		int index2 = SkinnedBodyIndices[Offset + index];
		int num = BonesOffset[index2];
		int num2 = BonesCount[index2];
		int2 @int = ParticlesOffsetCount[index2];
		float num3 = TeleportDistanceTreshold[index2];
		num3 *= num3;
		float4x4 a = LocalToWorldMatrices[index];
		for (int i = 0; i < num2; i++)
		{
			int index3 = i + num;
			int index4 = i + @int.x;
			float3 xyz = math.mul(a, Boneposes[index3]).c3.xyz;
			float num4 = math.distancesq(BasePosition[index4], xyz);
			if (Mass[index4] <= 0f || (num3 > 0f && num4 > num3))
			{
				Position[index4] = xyz;
			}
			BasePosition[index4] = xyz;
		}
	}
}
