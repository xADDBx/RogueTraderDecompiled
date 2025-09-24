using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Jobs;

[BurstCompile]
public struct UpdateMeshParticlesJob : IJobParallelFor
{
	[ReadOnly]
	public int Offset;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> MeshBodyIndices;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int2> ParticlesOffsetCount;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> VerticesOffset;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> VerticesCount;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float> TeleportDistanceTreshold;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4x4> LocalToWorldMatrices;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<Vector3> BaseVertices;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<ParticleExtendedData> ExtendedData;

	[NativeDisableParallelForRestriction]
	public NativeArray<ParticlePositionPair> PositionPairs;

	public void Execute(int index)
	{
		int index2 = MeshBodyIndices[Offset + index];
		int2 @int = ParticlesOffsetCount[index2];
		int num = VerticesOffset[index2];
		int num2 = VerticesCount[index2];
		float num3 = TeleportDistanceTreshold[index2];
		num3 *= num3;
		float4x4 a = LocalToWorldMatrices[index];
		for (int i = 0; i < num2; i++)
		{
			int index3 = i + @int.x;
			float3 xyz = BaseVertices[i + num];
			float3 xyz2 = math.mul(a, new float4(xyz, 1f)).xyz;
			float num4 = math.distancesq(PositionPairs[index3].BasePosition, xyz2);
			ParticlePositionPair value = PositionPairs[index3];
			if (ExtendedData[index3].Mass <= 0f || (num3 > 0f && num4 > num3))
			{
				value.Position = xyz2;
			}
			value.BasePosition = xyz2;
			PositionPairs[index3] = value;
		}
	}
}
