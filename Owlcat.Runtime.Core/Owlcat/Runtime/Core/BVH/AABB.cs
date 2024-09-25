using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.BVH;

[BurstCompile]
public struct AABB
{
	private static AABB s_Empty = new AABB(float.MaxValue, float.MinValue);

	public float3 Min;

	public float Padding0;

	public float3 Max;

	public float Padding1;

	public static AABB Empty => s_Empty;

	public float HalfArea
	{
		get
		{
			float3 @float = Max - Min;
			return @float.x * @float.y + @float.y * @float.z + @float.z * @float.x;
		}
	}

	public float3 Center => 0.5f * (Min + Max);

	public float3 Extents => Max - Min;

	public float3 HalfExtents => 0.5f * (Max - Min);

	public AABB(float3 min, float3 max)
	{
		Min = min;
		Max = max;
		Padding0 = (Padding1 = 0f);
	}

	public static implicit operator Bounds(AABB aabb)
	{
		return new Bounds(aabb.Center, aabb.Extents);
	}

	public static explicit operator AABB(Bounds b)
	{
		return new AABB(b.min, b.max);
	}

	public static AABB Union(in AABB a, in AABB b)
	{
		return new AABB(new float3(math.min(a.Min.x, b.Min.x), math.min(a.Min.y, b.Min.y), math.min(a.Min.z, b.Min.z)), new float3(math.max(a.Max.x, b.Max.x), math.max(a.Max.y, b.Max.y), math.max(a.Max.z, b.Max.z)));
	}

	public static bool Intersects(in AABB a, in AABB b)
	{
		if (a.Min.x <= b.Max.x && a.Max.x >= b.Min.x && a.Min.y <= b.Max.y && a.Max.y >= b.Min.y && a.Min.z <= b.Max.z)
		{
			return a.Max.z >= b.Min.z;
		}
		return false;
	}

	public void Include(float3 p)
	{
		Min.x = math.min(Min.x, p.x);
		Min.y = math.min(Min.y, p.y);
		Min.z = math.min(Min.z, p.z);
		Max.x = math.max(Max.x, p.x);
		Max.y = math.max(Max.y, p.y);
		Max.z = math.max(Max.z, p.z);
	}

	public void Expand(float r)
	{
		Min.x -= r;
		Min.y -= r;
		Min.z -= r;
		Max.x += r;
		Max.y += r;
		Max.z += r;
	}

	public void Expand(float3 r)
	{
		Min.x -= r.x;
		Min.y -= r.y;
		Min.z -= r.z;
		Max.x += r.x;
		Max.y += r.y;
		Max.z += r.z;
	}

	public bool Contains(in AABB rhs)
	{
		if (Min.x <= rhs.Min.x && Min.y <= rhs.Min.y && Min.z <= rhs.Min.z && Max.x >= rhs.Max.x && Max.y >= rhs.Max.y)
		{
			return Max.z >= rhs.Max.z;
		}
		return false;
	}

	public float RayCast(float3 from, float3 to, float maxFraction = 1f)
	{
		float num = float.MinValue;
		float num2 = float.MaxValue;
		float3 x = to - from;
		float3 @float = math.abs(x);
		for (int i = 0; i < 3; i++)
		{
			float num3 = x[i];
			float num4 = @float[i];
			float num5 = from[i];
			float num6 = Min[i];
			float num7 = Max[i];
			if (num4 < float.Epsilon)
			{
				if (num5 < num6 || num7 < num5)
				{
					return float.MinValue;
				}
				continue;
			}
			float num8 = 1f / num3;
			float num9 = (num6 - num5) * num8;
			float num10 = (num7 - num5) * num8;
			if (num9 > num10)
			{
				float num11 = num9;
				num9 = num10;
				num10 = num11;
			}
			num = Mathf.Max(num, num9);
			num2 = Mathf.Min(num2, num10);
			if (num > num2)
			{
				return float.MinValue;
			}
		}
		if (num < 0f)
		{
			return 0f;
		}
		if (maxFraction < num)
		{
			return float.MinValue;
		}
		return num;
	}
}
