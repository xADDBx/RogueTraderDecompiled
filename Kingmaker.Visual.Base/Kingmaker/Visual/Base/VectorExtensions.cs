using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.Visual.Base;

public static class VectorExtensions
{
	public static Vector2 xy(this Vector4 v)
	{
		return new Vector2(v.x, v.y);
	}

	public static Vector2 zw(this Vector4 v)
	{
		return new Vector2(v.z, v.w);
	}

	public static Vector3 MoveTowards(this Vector3 current, Vector3 target, float maxDistanceDelta, out float distance)
	{
		Vector3 vector = target - current;
		float sqrMagnitude = vector.sqrMagnitude;
		distance = Mathf.Sqrt(sqrMagnitude);
		if (Mathf.Approximately(sqrMagnitude, 0f) || (0f <= maxDistanceDelta && sqrMagnitude <= maxDistanceDelta * maxDistanceDelta))
		{
			return target;
		}
		return new Vector3(current.x + vector.x / distance * maxDistanceDelta, current.y + vector.y / distance * maxDistanceDelta, current.z + vector.z / distance * maxDistanceDelta);
	}

	public static bool MoveAlong([NotNull] this Transform[] transforms, float distance, ref Vector3 position, ref int pointIndex)
	{
		int num = transforms.Length;
		while (pointIndex < num)
		{
			position = position.MoveTowards(transforms[pointIndex].position, distance, out var distance2);
			distance -= distance2;
			if (distance < Mathf.Epsilon)
			{
				return false;
			}
			pointIndex++;
		}
		position = ((0 < num) ? transforms[num - 1].position : position);
		pointIndex = num;
		return true;
	}
}
