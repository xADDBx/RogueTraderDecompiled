using System;
using Unity.Burst;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Utilities;

[BurstCompile]
public struct Frustum
{
	[BurstCompile]
	public struct Planes
	{
		public Plane Plane0;

		public Plane Plane1;

		public Plane Plane2;

		public Plane Plane3;

		public Plane Plane4;

		public Plane Plane5;

		public Plane this[int index]
		{
			get
			{
				return index switch
				{
					0 => Plane0, 
					1 => Plane1, 
					2 => Plane2, 
					3 => Plane3, 
					4 => Plane4, 
					5 => Plane5, 
					_ => throw new IndexOutOfRangeException(), 
				};
			}
			set
			{
				switch (index)
				{
				case 0:
					Plane0 = value;
					break;
				case 1:
					Plane1 = value;
					break;
				case 2:
					Plane2 = value;
					break;
				case 3:
					Plane3 = value;
					break;
				case 4:
					Plane4 = value;
					break;
				case 5:
					Plane5 = value;
					break;
				default:
					throw new IndexOutOfRangeException();
				}
			}
		}
	}

	[BurstCompile]
	public struct Corners
	{
		public Vector3 Corner0;

		public Vector3 Corner1;

		public Vector3 Corner2;

		public Vector3 Corner3;

		public Vector3 Corner4;

		public Vector3 Corner5;

		public Vector3 Corner6;

		public Vector3 Corner7;

		public Vector3 this[int index]
		{
			get
			{
				return index switch
				{
					0 => Corner0, 
					1 => Corner1, 
					2 => Corner2, 
					3 => Corner3, 
					4 => Corner4, 
					5 => Corner5, 
					6 => Corner6, 
					7 => Corner7, 
					_ => throw new IndexOutOfRangeException(), 
				};
			}
			set
			{
				switch (index)
				{
				case 0:
					Corner0 = value;
					break;
				case 1:
					Corner1 = value;
					break;
				case 2:
					Corner2 = value;
					break;
				case 3:
					Corner3 = value;
					break;
				case 4:
					Corner4 = value;
					break;
				case 5:
					Corner5 = value;
					break;
				case 6:
					Corner6 = value;
					break;
				case 7:
					Corner7 = value;
					break;
				default:
					throw new IndexOutOfRangeException();
				}
			}
		}
	}

	private static Plane[] s_TmpPlanes = new Plane[6];

	public Planes planes;

	public Corners corners;

	private static Vector3 IntersectFrustumPlanes(Plane p0, Plane p1, Plane p2)
	{
		Vector3 normal = p0.normal;
		Vector3 normal2 = p1.normal;
		Vector3 normal3 = p2.normal;
		float num = Vector3.Dot(Vector3.Cross(normal, normal2), normal3);
		return (Vector3.Cross(normal3, normal2) * p0.distance + Vector3.Cross(normal, normal3) * p1.distance - Vector3.Cross(normal, normal2) * p2.distance) * (1f / num);
	}

	public static void Create(ref Frustum frustum, Matrix4x4 viewProjMatrix, Vector3 viewPos, Vector3 viewDir, float nearClipPlane, float farClipPlane)
	{
		GeometryUtility.CalculateFrustumPlanes(viewProjMatrix, s_TmpPlanes);
		for (int i = 0; i < 4; i++)
		{
			frustum.planes[i] = s_TmpPlanes[i];
		}
		Plane value = default(Plane);
		value.SetNormalAndPosition(viewDir, viewPos);
		value.distance -= nearClipPlane;
		Plane value2 = default(Plane);
		value2.SetNormalAndPosition(-viewDir, viewPos);
		value2.distance += farClipPlane;
		frustum.planes[4] = value;
		frustum.planes[5] = value2;
		frustum.corners[0] = IntersectFrustumPlanes(frustum.planes[0], frustum.planes[3], frustum.planes[4]);
		frustum.corners[1] = IntersectFrustumPlanes(frustum.planes[1], frustum.planes[3], frustum.planes[4]);
		frustum.corners[2] = IntersectFrustumPlanes(frustum.planes[0], frustum.planes[2], frustum.planes[4]);
		frustum.corners[3] = IntersectFrustumPlanes(frustum.planes[1], frustum.planes[2], frustum.planes[4]);
		frustum.corners[4] = IntersectFrustumPlanes(frustum.planes[0], frustum.planes[3], frustum.planes[5]);
		frustum.corners[5] = IntersectFrustumPlanes(frustum.planes[1], frustum.planes[3], frustum.planes[5]);
		frustum.corners[6] = IntersectFrustumPlanes(frustum.planes[0], frustum.planes[2], frustum.planes[5]);
		frustum.corners[7] = IntersectFrustumPlanes(frustum.planes[1], frustum.planes[2], frustum.planes[5]);
	}
}
