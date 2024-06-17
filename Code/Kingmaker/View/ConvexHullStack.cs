using System.Collections.Generic;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View;

public class ConvexHullStack
{
	[NotNull]
	public readonly List<Vector3> Points = new List<Vector3>();

	public readonly int Direction;

	public ConvexHullStack(int direction)
	{
		Direction = direction;
	}

	public void Clear()
	{
		Points.Clear();
	}

	public Vector3 Peek()
	{
		return Points[Points.Count - 1];
	}

	public void Push(Vector3 point)
	{
		if (Points.Count < 2)
		{
			Points.Add(point);
			return;
		}
		while (Points.Count > 1)
		{
			Vector3 vector = Points[Points.Count - 1];
			Vector3 vector2 = Points[Points.Count - 2];
			if (GeometryUtils.SignedAngle((vector - vector2).To2D(), (point - vector).To2D()) * (float)Direction <= 0f)
			{
				Points.RemoveAt(Points.Count - 1);
				continue;
			}
			bool flag = false;
			for (int i = 1; i < Points.Count - 1; i++)
			{
				if (VectorMath.SegmentsIntersectXZ(Points[i - 1], Points[i], vector, point))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				break;
			}
			Points.RemoveAt(Points.Count - 1);
		}
		Points.Add(point);
	}
}
