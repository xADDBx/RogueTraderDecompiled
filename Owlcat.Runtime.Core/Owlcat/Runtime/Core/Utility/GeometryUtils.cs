using System;
using UnityEngine;

namespace Owlcat.Runtime.Core.Utility;

public static class GeometryUtils
{
	public static bool ContainsXZ(this Bounds bounds, Vector3 point)
	{
		Vector3 vector = point - bounds.center;
		Vector3 extents = bounds.extents;
		if (0f - extents.x <= vector.x + 0.1f && extents.x >= vector.x - 0.1f && 0f - extents.z <= vector.z + 0.1f)
		{
			return extents.z >= vector.z - 0.1f;
		}
		return false;
	}

	public static Vector2 To2D(this Vector3 v)
	{
		return new Vector2(v.x, v.z);
	}

	public static Vector3 To3D(this Vector2 v)
	{
		return new Vector3(v.x, 0f, v.y);
	}

	public static Vector3 To3D(this Vector2 v, float y)
	{
		return new Vector3(v.x, y, v.y);
	}

	public static Vector3 ToXZ(this Vector3 v)
	{
		v.y = 0f;
		return v;
	}

	public static float SignedAngle(Vector2 a, Vector2 b)
	{
		float num = Vector2.Angle(a, b);
		if (!(Cross2D(a, b) > 0f))
		{
			return num;
		}
		return 0f - num;
	}

	public static float Cross2D(Vector2 a, Vector2 b)
	{
		return a.x * b.y - a.y * b.x;
	}

	public static float Distance2D(Vector3 a, Vector3 b)
	{
		float num = a.x - b.x;
		float num2 = a.z - b.z;
		return Mathf.Sqrt(num * num + num2 * num2);
	}

	public static Vector2 RotateAroundPoint(this Vector2 v, Vector2 origin, float theta)
	{
		float x = origin.x;
		float y = origin.y;
		float x2 = v.x;
		float y2 = v.y;
		float num = Mathf.Sin(theta * (MathF.PI / 180f));
		float num2 = Mathf.Cos(theta * (MathF.PI / 180f));
		float num3 = x2 - x;
		float num4 = y2 - y;
		float num5 = (float)((double)num3 * (double)num2 + (double)num4 * (double)num);
		float num6 = (float)((0.0 - (double)num3) * (double)num + (double)num4 * (double)num2);
		return new Vector2(num5 + x, num6 + y);
	}

	public static float ManhattanDistance2D(Vector3 a, Vector3 b)
	{
		float num = a.x - b.x;
		float num2 = a.z - b.z;
		return num + num2;
	}

	public static float ManhattanDistance2D(Vector3 a, Bounds b)
	{
		float a2 = Mathf.Abs(a.x - b.center.x) - b.size.x / 2f;
		float b2 = Mathf.Abs(a.z - b.center.z) - b.size.z / 2f;
		return Mathf.Max(a2, b2);
	}

	public static float SqrDistance2D(Vector3 a, Vector3 b)
	{
		float num = a.x - b.x;
		float num2 = a.z - b.z;
		return num * num + num2 * num2;
	}

	public static float MechanicsDistance(Vector3 a, Vector3 b)
	{
		float num = Mathf.Abs(a.y - b.y);
		float magnitude = (a - b).To2D().magnitude;
		if (num <= 1.5f)
		{
			return magnitude;
		}
		return Mathf.Max(magnitude, num / 2f);
	}

	public static float SqrMechanicsDistance(Vector3 a, Vector3 b)
	{
		float num = Mathf.Abs(a.y - b.y);
		float sqrMagnitude = (a - b).To2D().sqrMagnitude;
		if (num <= 1.5f)
		{
			return sqrMagnitude;
		}
		return Mathf.Max(sqrMagnitude, num * num / 4f);
	}

	public static Vector3 ProjectToGround(Vector3 p)
	{
		if (UnityEngine.Physics.Linecast(new Vector3(p.x, 5000f, p.z), new Vector3(p.x, -5000f, p.z), out var hitInfo, 2359553))
		{
			return hitInfo.point;
		}
		return p;
	}

	public static int GetWarhammerCellDistance(Vector3 a, Vector3 b, float gridCellSize)
	{
		Vector3Int vector3Int = Vector3Int.RoundToInt((b - a) / gridCellSize);
		int b2 = Mathf.Abs(vector3Int.z);
		int a2 = Mathf.Abs(vector3Int.x);
		int num = Mathf.Max(a2, b2);
		int num2 = Mathf.Min(a2, b2);
		return num + num2 / 2;
	}
}
