using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public static class VectorMathExtras
{
	public static bool SegmentsIntersect2D(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2, out float normDist1)
	{
		normDist1 = 0f;
		Vector2 vector = end1 - start1;
		Vector2 vector2 = end2 - start2;
		float num = vector2.y * vector.x - vector2.x * vector.y;
		if (num == 0f)
		{
			return false;
		}
		float num2 = vector2.x * (start1.y - start2.y) - vector2.y * (start1.x - start2.x);
		float num3 = vector.x * (start1.y - start2.y) - vector.y * (start1.x - start2.x);
		float num4 = num2 / num;
		float num5 = num3 / num;
		if (num4 < 0f || num4 > 1f || num5 < 0f || num5 > 1f)
		{
			return false;
		}
		normDist1 = num4;
		return true;
	}

	public static float SqrDistanceXZ(Int3 a, Int3 b)
	{
		Int3 @int = a - b;
		return (float)((long)@int.x * (long)@int.x + (long)@int.z * (long)@int.z) * 0.001f * 0.001f;
	}

	public static bool IsClockwiseMargin(Vector2 a, Vector2 b, Vector2 c)
	{
		return (b.x - a.x) * (c.y - a.y) - (c.x - a.x) * (b.y - a.y) <= Mathf.Epsilon;
	}

	public static bool ContainsSegmentXZ(Vector3[] polyPoints, Vector3 p1, Vector3 p2)
	{
		int num = polyPoints.Length - 1;
		bool flag = false;
		int num2 = 0;
		while (num2 < polyPoints.Length)
		{
			if (((polyPoints[num2].z <= p1.z && p1.z < polyPoints[num].z) || (polyPoints[num].z <= p1.z && p1.z < polyPoints[num2].z)) && p1.x < (polyPoints[num].x - polyPoints[num2].x) * (p1.z - polyPoints[num2].z) / (polyPoints[num].z - polyPoints[num2].z) + polyPoints[num2].x)
			{
				flag = !flag;
			}
			if (VectorMath.SegmentsIntersectXZ(polyPoints[num2], polyPoints[num], p1, p2))
			{
				return false;
			}
			num = num2++;
		}
		return flag;
	}

	public static float GetWindingXZ(Vector3[] points)
	{
		float num = 0f;
		for (int i = 0; i < points.Length; i++)
		{
			Vector3 vector = points[i];
			Vector3 vector2 = points[(i + 1) % points.Length];
			num += (vector2.x - vector.x) * (vector2.z + vector.z);
		}
		return num;
	}
}
