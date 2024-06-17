using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase.Jobs;

[BurstCompile]
public struct GridCalculateHashJob : IJobParallelFor
{
	[ReadOnly]
	public float3 CellSize;

	[ReadOnly]
	public NativeArray<float3> AabbMin;

	[ReadOnly]
	public NativeArray<float3> AabbMax;

	[ReadOnly]
	public NativeArray<float3> SceneAabb;

	[ReadOnly]
	public MultilevelGridDimension Dimension;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<KeyValuePairComparable<uint, uint>> Hash;

	public void Execute(int index)
	{
		float3 @float = SceneAabb[0];
		float3 float2 = AabbMin[index] - @float;
		float3 float3 = AabbMax[index] - @float;
		uint gridLevel = PBDMath.GridLevel(float3 - float2, CellSize);
		uint4 @uint = PBDMath.MultilevelGridPos(float2, gridLevel, CellSize);
		uint4 uint2 = PBDMath.MultilevelGridPos(float3, gridLevel, CellSize) - @uint;
		uint2.xyz = math.clamp(uint2.xyz + 1u, 1u, 2u);
		if (Dimension == MultilevelGridDimension.Grid2D)
		{
			uint2.y = 1u;
		}
		int num = 1 << (int)Dimension;
		int num2 = index * num;
		int num3 = 0;
		uint4 gridPos = @uint;
		for (uint num4 = 0u; num4 < uint2.z; num4++)
		{
			gridPos.z = @uint.z + num4;
			for (uint num5 = 0u; num5 < uint2.y; num5++)
			{
				if (Dimension == MultilevelGridDimension.Grid2D)
				{
					gridPos.y = 0u;
				}
				else
				{
					gridPos.y = @uint.y + num5;
				}
				for (uint num6 = 0u; num6 < uint2.x; num6++)
				{
					gridPos.x = @uint.x + num6;
					Hash[num2 + num3] = new KeyValuePairComparable<uint, uint>(PBDMath.MultilevelGridHash(gridPos), (uint)index);
					num3++;
				}
			}
		}
		for (int i = num3; i < num; i++)
		{
			Hash[num2 + i] = new KeyValuePairComparable<uint, uint>(uint.MaxValue, 0u);
		}
	}

	private void DebugHash(uint hashKey, uint4 gridPos)
	{
		char[] array = new char[32];
		int num = 0;
		while (hashKey != 0)
		{
			array[num++] = (((hashKey & 1) == 1) ? '1' : '0');
			hashKey >>= 1;
		}
		Array.Reverse(array);
		Debug.Log($"{gridPos} key: {new string(array)}");
	}
}
