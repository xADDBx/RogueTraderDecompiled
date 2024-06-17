using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

[BurstCompile]
public struct OBoxSatGeometry : ISatGeometry
{
	private readonly float3 axis0;

	private readonly float3 axis1;

	private readonly float3 axis2;

	private readonly float3 point0;

	private readonly float3 point1;

	private readonly float3 point2;

	private readonly float3 point3;

	private readonly float3 point4;

	private readonly float3 point5;

	private readonly float3 point6;

	private readonly float3 point7;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public OBoxSatGeometry(in OBox box)
	{
		axis0 = box.xAxis;
		axis1 = box.yAxis;
		axis2 = box.zAxis;
		float3 center = box.center;
		float3 @float = box.xAxis * box.extents.x;
		float3 float2 = box.yAxis * box.extents.y;
		float3 float3 = box.zAxis * box.extents.z;
		point0 = center + @float + float2 + float3;
		point1 = center + @float + float2 - float3;
		point2 = center + @float - float2 + float3;
		point3 = center + @float - float2 - float3;
		point4 = center - @float + float2 + float3;
		point5 = center - @float + float2 - float3;
		point6 = center - @float - float2 + float3;
		point7 = center - @float - float2 - float3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetFaceNormals(ref NativeSlice<float3> container)
	{
		container[0] = axis0;
		container[1] = axis1;
		container[2] = axis2;
		return 3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetEdgeDirections(ref NativeSlice<float3> container)
	{
		container[0] = axis0;
		container[1] = axis1;
		container[2] = axis2;
		return 3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ProjectToAxis(float3 axis, ref float min, ref float max)
	{
		min = (max = math.dot(axis, point0));
		float y = math.dot(axis, point1);
		max = math.max(max, y);
		min = math.min(min, y);
		y = math.dot(axis, point2);
		max = math.max(max, y);
		min = math.min(min, y);
		y = math.dot(axis, point3);
		max = math.max(max, y);
		min = math.min(min, y);
		y = math.dot(axis, point4);
		max = math.max(max, y);
		min = math.min(min, y);
		y = math.dot(axis, point5);
		max = math.max(max, y);
		min = math.min(min, y);
		y = math.dot(axis, point6);
		max = math.max(max, y);
		min = math.min(min, y);
		y = math.dot(axis, point7);
		max = math.max(max, y);
		min = math.min(min, y);
	}
}
