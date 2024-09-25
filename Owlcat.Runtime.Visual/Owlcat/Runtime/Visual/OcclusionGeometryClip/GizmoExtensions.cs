using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

public static class GizmoExtensions
{
	public static void DrawGizmos(this in ABox box)
	{
		Gizmos.DrawWireCube(box.Center, box.Extent * 2f);
	}

	public static void DrawGizmos(this in OBox box)
	{
		Matrix4x4 matrix = Gizmos.matrix;
		Gizmos.matrix = box.GetMatrix();
		Gizmos.DrawWireCube(default(Vector3), box.extents * 2f);
		Gizmos.matrix = matrix;
	}

	public static void DrawGizmos(this in Plane plane, float size = 5f, float3 origin = default(float3))
	{
		Matrix4x4 matrix = Gizmos.matrix;
		Color color = Gizmos.color;
		float num = (object.Equals(origin, null) ? 0f : GeometryMath.SignedDistance(in plane, in origin));
		Gizmos.matrix = Matrix4x4.TRS(origin + plane.normal * (0f - num), Quaternion.LookRotation(plane.normal), Vector3.one);
		Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.1f);
		Gizmos.DrawWireCube(Vector3.zero, new Vector3(size, size));
		Gizmos.color = new Color(color.r, color.g, color.b, color.a / 2f);
		Gizmos.DrawCube(Vector3.zero, new Vector3(size, size));
		Gizmos.DrawLine(Vector3.zero, Vector3.forward);
		Gizmos.matrix = matrix;
		Gizmos.color = color;
	}

	public static void DrawGizmos(this in Frustum frustum, bool drawPlanes = false, float planeSize = 1f)
	{
		Line line = GeometryMath.IntersectUnsafe(in frustum.bottom, in frustum.left);
		Line line2 = GeometryMath.IntersectUnsafe(in frustum.bottom, in frustum.right);
		Line line3 = GeometryMath.IntersectUnsafe(in frustum.top, in frustum.left);
		Line line4 = GeometryMath.IntersectUnsafe(in frustum.top, in frustum.right);
		float3 @float = GeometryMath.IntersectUnsafe(in frustum.near, in line);
		float3 float2 = GeometryMath.IntersectUnsafe(in frustum.near, in line2);
		float3 float3 = GeometryMath.IntersectUnsafe(in frustum.near, in line3);
		float3 float4 = GeometryMath.IntersectUnsafe(in frustum.near, in line4);
		float3 float5 = GeometryMath.IntersectUnsafe(in frustum.far, in line);
		float3 float6 = GeometryMath.IntersectUnsafe(in frustum.far, in line2);
		float3 float7 = GeometryMath.IntersectUnsafe(in frustum.far, in line3);
		float3 float8 = GeometryMath.IntersectUnsafe(in frustum.far, in line4);
		Gizmos.DrawLine(@float, float2);
		Gizmos.DrawLine(float3, float4);
		Gizmos.DrawLine(@float, float3);
		Gizmos.DrawLine(float2, float4);
		Gizmos.DrawLine(float5, float6);
		Gizmos.DrawLine(float7, float8);
		Gizmos.DrawLine(float5, float7);
		Gizmos.DrawLine(float6, float8);
		Gizmos.DrawLine(@float, float5);
		Gizmos.DrawLine(float2, float6);
		Gizmos.DrawLine(float3, float7);
		Gizmos.DrawLine(float4, float8);
		if (drawPlanes)
		{
			float3 origin = math.lerp(math.lerp(@float, float8, 0.5f), math.lerp(float4, float5, 0.5f), 0.5f);
			frustum.left.DrawGizmos(planeSize, origin);
			frustum.right.DrawGizmos(planeSize, origin);
			frustum.top.DrawGizmos(planeSize, origin);
			frustum.bottom.DrawGizmos(planeSize, origin);
			frustum.near.DrawGizmos(planeSize, origin);
			frustum.far.DrawGizmos(planeSize, origin);
		}
	}

	public static void DrawGizmos(this in FrustumSatGeometry f)
	{
		Color color = Gizmos.color;
		f.frustum.DrawGizmos();
		float radius = 0.1f;
		Gizmos.DrawSphere(f.point0, radius);
		Gizmos.DrawSphere(f.point1, radius);
		Gizmos.DrawSphere(f.point2, radius);
		Gizmos.DrawSphere(f.point3, radius);
		Gizmos.DrawSphere(f.point4, radius);
		Gizmos.color = new Color(color.r, color.g, color.b, 0.1f);
		f.bounds.DrawGizmos();
	}
}
