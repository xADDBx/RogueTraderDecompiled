using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

[BurstCompile]
public struct OBox : IEquatable<OBox>
{
	public const float kMinExtentComponent = 0.01f;

	public static readonly float3 kMinExtent = new float3(0.01f);

	public float3 center;

	public float3 extents;

	public float3 xAxis;

	public float3 yAxis;

	public float3 zAxis;

	public override string ToString()
	{
		return $"{{center:{center}, extents:{extents}, xAxis:{xAxis}, yAxis:{yAxis}, zAxis:{zAxis}}}";
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public float4x4 GetMatrix()
	{
		return float4x4.TRS(center, quaternion.LookRotation(zAxis, yAxis), new float3(1f, 1f, 1f));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public float3 TransformInverse(float3 p)
	{
		float3 x = p - center;
		return new float3(math.dot(x, xAxis), math.dot(x, yAxis), math.dot(x, zAxis));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public float3 Transform(float3 p)
	{
		return center + xAxis * p.x + yAxis * p.y + zAxis * p.z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public OBox GetTransformed(in float4x4 matrix)
	{
		float3 @float = math.transform(matrix, center);
		float3 float2 = math.rotate(matrix, xAxis);
		float3 float3 = math.rotate(matrix, yAxis);
		float3 float4 = math.rotate(matrix, zAxis);
		float num = math.length(float2);
		float num2 = math.length(float3);
		float num3 = math.length(float4);
		OBox result = default(OBox);
		result.center = @float;
		result.extents = extents * new float3(num, num2, num3);
		result.xAxis = float2 / num;
		result.yAxis = float3 / num2;
		result.zAxis = float4 / num3;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public OBox GetTransformedSafe(in float4x4 matrix)
	{
		float3 @float = math.transform(matrix, center);
		float3 x = math.rotate(matrix, xAxis);
		float3 x2 = math.rotate(matrix, yAxis);
		float3 x3 = math.rotate(matrix, zAxis);
		float x4 = math.length(x);
		float y = math.length(x2);
		float z = math.length(x3);
		OBox result = default(OBox);
		result.center = @float;
		result.extents = math.max(extents * new float3(x4, y, z), kMinExtent);
		result.xAxis = math.normalize(x);
		result.yAxis = math.normalize(x2);
		result.zAxis = math.normalize(x3);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public ABox GetBounds()
	{
		float3 @float = center;
		float3 float2 = xAxis * extents.x;
		float3 float3 = yAxis * extents.y;
		float3 float4 = zAxis * extents.z;
		float3 x;
		float3 x2 = (x = @float + float2 + float3 + float4);
		float3 y = @float + float2 + float3 - float4;
		x2 = math.min(x2, y);
		x = math.max(x, y);
		y = @float + float2 - float3 + float4;
		x2 = math.min(x2, y);
		x = math.max(x, y);
		y = @float + float2 - float3 - float4;
		x2 = math.min(x2, y);
		x = math.max(x, y);
		y = @float - float2 + float3 + float4;
		x2 = math.min(x2, y);
		x = math.max(x, y);
		y = @float - float2 + float3 - float4;
		x2 = math.min(x2, y);
		x = math.max(x, y);
		y = @float - float2 - float3 + float4;
		x2 = math.min(x2, y);
		x = math.max(x, y);
		y = @float - float2 - float3 - float4;
		x2 = math.min(x2, y);
		x = math.max(x, y);
		return new ABox(in x2, in x);
	}

	public bool Equals(OBox other)
	{
		if (center.Equals(other.center) && extents.Equals(other.extents) && xAxis.Equals(other.xAxis) && yAxis.Equals(other.yAxis))
		{
			return zAxis.Equals(other.zAxis);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is OBox other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(center, extents, xAxis, yAxis, zAxis);
	}
}
