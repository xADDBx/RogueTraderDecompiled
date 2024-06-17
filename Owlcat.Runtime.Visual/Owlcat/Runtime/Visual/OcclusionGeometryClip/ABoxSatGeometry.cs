using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

public struct ABoxSatGeometry : ISatGeometry
{
	public float3 min;

	public float3 max;

	public ABoxSatGeometry(in ABox box)
	{
		min = box.min;
		max = box.max;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetFaceNormals(ref NativeSlice<float3> container)
	{
		container[0] = new float3(1f, 0f, 0f);
		container[1] = new float3(0f, 1f, 0f);
		container[2] = new float3(0f, 0f, 1f);
		return 3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetEdgeDirections(ref NativeSlice<float3> container)
	{
		container[0] = new float3(1f, 0f, 0f);
		container[1] = new float3(0f, 1f, 0f);
		container[2] = new float3(0f, 0f, 1f);
		return 3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ProjectToAxis(float3 axis, ref float axisMin, ref float axisMax)
	{
		float3 @float = (max - min) / 2f;
		float3 y = min + @float;
		float num = math.csum(@float * math.abs(axis));
		float num2 = math.dot(axis, y);
		axisMin = num2 - num;
		axisMax = num2 + num;
	}
}
