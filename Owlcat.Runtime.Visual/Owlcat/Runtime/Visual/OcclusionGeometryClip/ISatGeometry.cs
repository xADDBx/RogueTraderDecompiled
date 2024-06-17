using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

internal interface ISatGeometry
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	int GetFaceNormals(ref NativeSlice<float3> container);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	int GetEdgeDirections(ref NativeSlice<float3> container);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void ProjectToAxis(float3 axis, ref float axisMin, ref float axisMax);
}
