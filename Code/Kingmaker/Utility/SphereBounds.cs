using UnityEngine;

namespace Kingmaker.Utility;

public readonly struct SphereBounds
{
	public readonly Vector3 Center;

	public readonly float RadiusSqr;

	public SphereBounds(Vector3 center, float radius)
	{
		Center = center;
		RadiusSqr = radius * radius;
	}

	public bool Contains(in Vector3 point)
	{
		return Vector3.SqrMagnitude(Center - point) <= RadiusSqr;
	}
}
