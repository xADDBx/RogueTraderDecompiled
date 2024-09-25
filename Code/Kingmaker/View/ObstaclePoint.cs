using System;
using UnityEngine;

namespace Kingmaker.View;

internal struct ObstaclePoint
{
	public float Angle;

	public ObstaclePointType Type;

	public ObstacleMode Mode;

	public static Comparison<ObstaclePoint> Comparer { get; } = (ObstaclePoint a, ObstaclePoint b) => a.CompareTo(b);


	public ObstaclePoint(float angle, ObstaclePointType type, ObstacleMode mode)
	{
		Angle = angle;
		Type = type;
		Mode = mode;
	}

	public int CompareTo(ObstaclePoint other)
	{
		if (Angle != other.Angle)
		{
			return Mathf.Abs(Angle).CompareTo(Mathf.Abs(other.Angle));
		}
		int type = (int)other.Type;
		return type.CompareTo((int)Type);
	}
}
