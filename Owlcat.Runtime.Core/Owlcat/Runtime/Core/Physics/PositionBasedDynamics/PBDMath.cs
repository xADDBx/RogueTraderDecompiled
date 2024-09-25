using System;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics;

public static class PBDMath
{
	public const uint k2BitMask = 3u;

	public const uint k10BitMask = 1023u;

	public const uint kMultilevelGridEmptyHash = uint.MaxValue;

	private static int[] m_PrimeNumbers = new int[29]
	{
		2, 5, 11, 17, 37, 67, 131, 257, 521, 1031,
		2053, 4099, 8209, 16411, 20261, 26407, 32771, 40577, 50021, 65539,
		80051, 95101, 110017, 120011, 131101, 262147, 524309, 1048583, 2097169
	};

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float3 NearestPointOnLine(float3 linePoint, float3 lineDir, float3 point)
	{
		lineDir = math.normalize(lineDir);
		float num = math.dot(point - linePoint, lineDir);
		return linePoint + lineDir * num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static quaternion FromToRotation(float3 from, float3 to)
	{
		float3 x = math.normalize(from);
		float3 y = math.normalize(to);
		float num = math.dot(x, y);
		float angle = math.acos(num);
		float3 x2 = math.cross(x, y);
		if (math.abs(1f + num) < 1E-06f)
		{
			angle = MathF.PI;
			x2 = ((!(x.x > x.y) || !(x.x > x.z)) ? math.cross(x, new float3(1f, 0f, 0f)) : math.cross(x, new float3(0f, 1f, 0f)));
		}
		else if (math.abs(1f - num) < 1E-06f)
		{
			return quaternion.identity;
		}
		x2 = math.normalize(x2);
		return quaternion.AxisAngle(x2, angle);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void TransformAABB(ref float3 aabbMin, ref float3 aabbMax, ref float4x4 matrix)
	{
		float3 xyz = (aabbMin + aabbMax) * 0.5f;
		float3 b = (aabbMax - aabbMin) * 0.5f;
		float4x4 a = matrix;
		a.c0.xyz = math.abs(a.c0.xyz);
		a.c1.xyz = math.abs(a.c1.xyz);
		a.c2.xyz = math.abs(a.c2.xyz);
		xyz = math.mul(a, new float4(xyz, 1f)).xyz;
		b = math.rotate(a, b);
		aabbMin = xyz - b;
		aabbMax = xyz + b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float4 CreatePlane(float3 position, float3 normal)
	{
		float w = 0f - math.dot(position, normal);
		return new float4(normal, w);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint GridLevel(float3 aabbSize, float3 cellSize)
	{
		float3 @float = aabbSize / cellSize;
		return math.clamp((uint)math.ceil(math.log2((uint)math.ceil(math.max(@float.x, math.max(@float.y, @float.z))))), 0u, 3u);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint4 MultilevelGridPos(float3 gridLocalPos, uint gridLevel, float3 cellSize)
	{
		float3 x = cellSize * (1 << (int)gridLevel);
		uint4 result = 0u;
		result.xyz = (uint3)math.floor(gridLocalPos * math.rcp(x));
		result.w = gridLevel;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint MultilevelGridHash(uint4 gridPos)
	{
		return ((gridPos.w & 3) << 30) | ((gridPos.x & 0x3FF) << 20) | ((gridPos.y & 0x3FF) << 10) | (gridPos.z & 0x3FFu);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint MultilevelGridHash(float3 gridLocalPos, uint gridLevel, float3 cellSize)
	{
		return MultilevelGridHash(MultilevelGridPos(gridLocalPos, gridLevel, cellSize));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint MultilevelGridHash(float3 aabbSize, float3 gridLocalPos, float3 cellSize)
	{
		return MultilevelGridHash(MultilevelGridPos(gridLocalPos, GridLevel(aabbSize, cellSize), cellSize));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool TestAABBOverlap(float3 min0, float3 max0, float3 min1, float3 max1)
	{
		if (min0.x <= max1.x && min1.x <= max0.x && min0.y <= max1.y && min1.y <= max0.y && min0.z <= max1.z)
		{
			return min1.z <= max0.z;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int NextPrimeNumber(int value)
	{
		for (int i = 0; i < m_PrimeNumbers.Length; i++)
		{
			if (m_PrimeNumbers[i] > value)
			{
				return m_PrimeNumbers[i];
			}
		}
		return m_PrimeNumbers[m_PrimeNumbers.Length - 1];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint SpatialHash(int3 discretePosition)
	{
		return (uint)((discretePosition.x * 73856093) ^ (discretePosition.y * 19349663) ^ (discretePosition.z * 83492791));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int3 SpatialDiscretizePosition(float3 position, float invCellSize)
	{
		return (int3)math.floor(position * invCellSize);
	}
}
