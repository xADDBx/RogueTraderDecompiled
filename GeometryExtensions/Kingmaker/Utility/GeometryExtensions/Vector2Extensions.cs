using UnityEngine;

namespace Kingmaker.Utility.GeometryExtensions;

public static class Vector2Extensions
{
	public static bool IsApproximatelyZero(this Vector2 value)
	{
		if (Mathf.Approximately(value.x, 0f))
		{
			return Mathf.Approximately(value.y, 0f);
		}
		return false;
	}

	public static bool Approximately(this Vector2 a, Vector2 b)
	{
		if (Mathf.Approximately(a.x, b.x))
		{
			return Mathf.Approximately(a.y, b.y);
		}
		return false;
	}

	public static Vector2 RotateTowards(this Vector2 current, Vector2 target, float maxRadiansDelta, float maxMagnitudeDelta)
	{
		return Vector3.RotateTowards(current, target, maxRadiansDelta, maxMagnitudeDelta);
	}

	public static bool IsDiagonal(this Vector2 a, Vector2 b)
	{
		Vector2 vector = b - a;
		float sqrMagnitude = vector.sqrMagnitude;
		float x = vector.x;
		float num = x * x;
		if (0.1464466f * sqrMagnitude < num)
		{
			return num < 0.85355335f * sqrMagnitude;
		}
		return false;
	}
}
