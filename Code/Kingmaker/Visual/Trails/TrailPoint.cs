using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.Trails;

public class TrailPoint
{
	private static Stack<TrailPoint> s_Cache = new Stack<TrailPoint>();

	public Vector3 Position;

	public Vector3 Right;

	public Vector3 ViewRight;

	public Vector3 Normal;

	public Vector3 Velocity;

	public float Lifetime;

	public float PositionInTrail;

	public float PositionFromStart;

	private TrailPoint()
	{
	}

	public static TrailPoint Pop()
	{
		if (s_Cache.Count > 0)
		{
			TrailPoint trailPoint = s_Cache.Pop();
			trailPoint.PositionFromStart = 0f;
			trailPoint.PositionInTrail = 0f;
			return trailPoint;
		}
		return new TrailPoint();
	}

	public static void Push(TrailPoint point)
	{
		s_Cache.Push(point);
	}
}
