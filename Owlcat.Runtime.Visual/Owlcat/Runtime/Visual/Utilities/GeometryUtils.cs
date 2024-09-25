using UnityEngine;

namespace Owlcat.Runtime.Visual.Utilities;

public static class GeometryUtils
{
	public unsafe static bool Overlap(OrientedBBox obb, Frustum frustum, int numPlanes, int numCorners)
	{
		bool flag = true;
		int num = 0;
		while (flag && num < numPlanes)
		{
			Vector3 normal = frustum.planes[num].normal;
			float distance = frustum.planes[num].distance;
			float num2 = obb.extentX * Mathf.Abs(Vector3.Dot(normal, obb.right)) + obb.extentY * Mathf.Abs(Vector3.Dot(normal, obb.up)) + obb.extentZ * Mathf.Abs(Vector3.Dot(normal, obb.forward));
			float num3 = Vector3.Dot(normal, obb.center) + distance;
			flag = flag && num2 + num3 >= 0f;
			num++;
		}
		if (numCorners == 0)
		{
			return flag;
		}
		Plane* ptr = stackalloc Plane[3];
		ptr->normal = obb.right;
		ptr->distance = obb.extentX;
		ptr[1].normal = obb.up;
		ptr[1].distance = obb.extentY;
		ptr[2].normal = obb.forward;
		ptr[2].distance = obb.extentZ;
		int num4 = 0;
		while (flag && num4 < 3)
		{
			Plane plane = ptr[num4];
			bool flag2 = true;
			bool flag3 = true;
			for (int i = 0; i < numCorners; i++)
			{
				float num5 = Vector3.Dot(plane.normal, frustum.corners[i] - obb.center);
				flag2 = flag2 && num5 > plane.distance;
				flag3 = flag3 && 0f - num5 > plane.distance;
			}
			flag = flag && !(flag2 || flag3);
			num4++;
		}
		return flag;
	}

	public static bool Intersects(OrientedBBox obb, Ray ray)
	{
		Vector3 rhs = obb.center - ray.origin;
		Vector3 vector = new Vector3(Vector3.Dot(obb.right, ray.direction), Vector3.Dot(obb.up, ray.direction), Vector3.Dot(obb.forward, ray.direction));
		Vector3 vector2 = new Vector3(Vector3.Dot(obb.right, rhs), Vector3.Dot(obb.up, rhs), Vector3.Dot(obb.forward, rhs));
		Vector3 vector3 = new Vector3(obb.extentX, obb.extentY, obb.extentZ);
		Vector3 vector4 = default(Vector3);
		Vector3 vector5 = default(Vector3);
		for (int i = 0; i < 3; i++)
		{
			if (Mathf.Approximately(vector[i], 0f))
			{
				if (0f - vector2[i] - vector3[i] > 0f || 0f - vector2[i] + vector3[i] < 0f)
				{
					return false;
				}
				vector[i] = 1E-05f;
			}
			vector4[i] = (vector2[i] + vector3[i]) / vector[i];
			vector5[i] = (vector2[i] - vector3[i]) / vector[i];
		}
		float num = Mathf.Max(Mathf.Max(Mathf.Min(vector4[0], vector5[0]), Mathf.Min(vector4[1], vector5[1])), Mathf.Min(vector4[2], vector5[2]));
		float num2 = Mathf.Min(Mathf.Min(Mathf.Max(vector4[0], vector5[0]), Mathf.Max(vector4[1], vector5[1])), Mathf.Max(vector4[2], vector5[2]));
		if (num2 < 0f)
		{
			return false;
		}
		if (num > num2)
		{
			return false;
		}
		return true;
	}
}
