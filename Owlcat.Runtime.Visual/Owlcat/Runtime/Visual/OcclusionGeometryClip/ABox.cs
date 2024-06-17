using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

[BurstCompile]
public struct ABox
{
	private float3 m_Min;

	private float3 m_Max;

	private float3 m_Center;

	private float3 m_Extent;

	public float3 min
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Min;
		}
	}

	public float3 max
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Max;
		}
	}

	public float3 Center
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Center;
		}
	}

	public float3 Extent
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Extent;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ABox(in float3 min, in float3 max)
	{
		m_Min = min;
		m_Max = max;
		m_Center = math.lerp(min, max, 0.5f);
		m_Extent = (max - min) / 2f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetMin(float min, int axis)
	{
		m_Min[axis] = min;
		m_Center[axis] = math.lerp(min, max[axis], 0.5f);
		m_Extent[axis] = (max[axis] - min) / 2f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetMax(float max, int axis)
	{
		m_Max[axis] = max;
		m_Center[axis] = math.lerp(min[axis], max, 0.5f);
		m_Extent[axis] = (max - min[axis]) / 2f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ABox Lerp(in ABox a, in ABox b, float t)
	{
		float3 @float = math.lerp(a.m_Min, b.m_Min, t);
		float3 float2 = math.lerp(a.m_Max, b.m_Max, t);
		return new ABox(in @float, in float2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ProjectToAxis(float3 axis, out float axisMin, out float axisMax)
	{
		float3 @float = (m_Max - m_Min) / 2f;
		float3 y = m_Min + @float;
		float num = math.csum(@float * math.abs(axis));
		float num2 = math.dot(axis, y);
		axisMin = num2 - num;
		axisMax = num2 + num;
	}

	public override string ToString()
	{
		return $"{{min:{m_Min}, max:{m_Max}, c:{m_Center}, e:{m_Extent}}}";
	}
}
