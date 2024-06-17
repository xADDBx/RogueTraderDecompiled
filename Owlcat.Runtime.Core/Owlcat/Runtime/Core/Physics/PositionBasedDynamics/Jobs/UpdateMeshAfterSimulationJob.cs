using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Jobs;

[BurstCompile]
public struct UpdateMeshAfterSimulationJob : IJobParallelFor
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
	public NativeArray<int> IndicesOffset;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> IndicesCount;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> VerticesOffset;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> VerticesCount;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> VertexTriangleMapOffset;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> VertexTriangleMapOffsetCountOffset;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4x4> WorldToLocalMatrices;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> Indices;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> VertexTriangleMap;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> VertexTriangleMapOffsetCount;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<Vector2> Uvs;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<Vector3> Vertices;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<Vector3> Normals;

	[NativeDisableParallelForRestriction]
	public NativeArray<Vector4> Tangents;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> Position;

	public void Execute(int entryIndex)
	{
		int index = MeshBodyIndices[Offset + entryIndex];
		int2 @int = ParticlesOffsetCount[index];
		int num = IndicesOffset[index];
		int num2 = IndicesCount[index] / 3;
		int num3 = VerticesOffset[index];
		int num4 = VerticesCount[index];
		int num5 = VertexTriangleMapOffsetCountOffset[index];
		int num6 = VertexTriangleMapOffset[index];
		NativeArray<float3> nativeArray = new NativeArray<float3>(1280, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
		NativeArray<float3> nativeArray2 = new NativeArray<float3>(1280, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
		float4x4 a = WorldToLocalMatrices[entryIndex];
		for (int i = 0; i < num2; i++)
		{
			int num7 = i * 3 + num;
			int num8 = Indices[num7];
			int num9 = Indices[num7 + 1];
			int num10 = Indices[num7 + 2];
			float3 xyz = Position[num8 + @int.x];
			float3 xyz2 = Position[num9 + @int.x];
			float3 xyz3 = Position[num10 + @int.x];
			xyz = math.mul(a, new float4(xyz, 1f)).xyz;
			xyz2 = math.mul(a, new float4(xyz2, 1f)).xyz;
			xyz3 = math.mul(a, new float4(xyz3, 1f)).xyz;
			float2 @float = Uvs[num8 + num3];
			float2 float2 = Uvs[num9 + num3];
			float2 float3 = Uvs[num10 + num3];
			float3 float4 = xyz2 - xyz;
			float3 float5 = xyz3 - xyz;
			nativeArray[i] = math.cross(float4, float5);
			float3 float6 = float4;
			float3 float7 = float5;
			float2 float8 = float2 - @float;
			float2 float9 = float3 - @float;
			float num11 = float8.x * float9.y - float8.y * float9.x;
			if (num11 == 0f)
			{
				nativeArray2[i] = 0;
				continue;
			}
			float num12 = 1f / num11;
			float3 value = new float3(float6.x * float9.y + float7.x * (0f - float8.y), float6.y * float9.y + float7.y * (0f - float8.y), float6.z * float9.y + float7.z * (0f - float8.y)) * num12;
			nativeArray2[i] = value;
		}
		for (int j = 0; j < num4; j++)
		{
			int index2 = j + num3;
			int num13 = j * 2 + num5;
			int num14 = VertexTriangleMapOffsetCount[num13];
			int num15 = VertexTriangleMapOffsetCount[num13 + 1];
			float3 float10 = 0;
			float3 float11 = 0;
			for (int k = 0; k < num15; k++)
			{
				int index3 = VertexTriangleMap[k + num14 + num6];
				float10 += nativeArray[index3];
				float11 += nativeArray2[index3];
			}
			if (math.all(float10 != 0f) && math.all(float11 != 0f))
			{
				float10 = math.normalize(float10);
				float11 = math.normalize(float11);
				Normals[index2] = float10;
				Vector4 value2 = Tangents[index2];
				value2.x = float11.x;
				value2.y = float11.y;
				value2.z = float11.z;
				Tangents[index2] = value2;
			}
			Vertices[index2] = math.mul(a, new float4(Position[j + @int.x], 1f)).xyz;
		}
		nativeArray.Dispose();
		nativeArray2.Dispose();
	}
}
