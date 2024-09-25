using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.Highlighting;

[BurstCompile]
internal struct CullingJob : IJobParallelFor
{
	public NativeArray<BoundsVisibility> Bounds;

	[ReadOnly]
	public NativeArray<Plane> CameraPlanes;

	public void Execute(int index)
	{
		BoundsVisibility value = Bounds[index];
		value.Visibility = TestPlanesAABBInternalFast(ref CameraPlanes, ref value.Bounds);
		Bounds[index] = value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TestPlanesResults TestPlanesAABBInternalFast(ref NativeArray<Plane> planes, ref Bounds bounds)
	{
		Vector3 boundsMin = bounds.min;
		Vector3 boundsMax = bounds.max;
		return TestPlanesAABBInternalFast(ref planes, ref boundsMin, ref boundsMax);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TestPlanesResults TestPlanesAABBInternalFast(ref NativeArray<Plane> planes, ref Vector3 boundsMin, ref Vector3 boundsMax, bool testIntersection = false)
	{
		TestPlanesResults result = TestPlanesResults.Inside;
		Vector3 vector = default(Vector3);
		Vector3 vector2 = default(Vector3);
		for (int i = 0; i < planes.Length; i++)
		{
			Vector3 normal = planes[i].normal;
			float distance = planes[i].distance;
			if (normal.x < 0f)
			{
				vector.x = boundsMin.x;
				vector2.x = boundsMax.x;
			}
			else
			{
				vector.x = boundsMax.x;
				vector2.x = boundsMin.x;
			}
			if (normal.y < 0f)
			{
				vector.y = boundsMin.y;
				vector2.y = boundsMax.y;
			}
			else
			{
				vector.y = boundsMax.y;
				vector2.y = boundsMin.y;
			}
			if (normal.z < 0f)
			{
				vector.z = boundsMin.z;
				vector2.z = boundsMax.z;
			}
			else
			{
				vector.z = boundsMax.z;
				vector2.z = boundsMin.z;
			}
			if (normal.x * vector.x + normal.y * vector.y + normal.z * vector.z + distance < 0f)
			{
				return TestPlanesResults.Outside;
			}
			if (testIntersection && normal.x * vector2.x + normal.y * vector2.y + normal.z * vector2.z + distance <= 0f)
			{
				result = TestPlanesResults.Intersect;
			}
		}
		return result;
	}
}
