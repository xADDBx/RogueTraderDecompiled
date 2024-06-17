using System;
using System.Collections.Generic;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.Random;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View;

public class FreePlaceSelector
{
	private const int MaxIterations = 30;

	private const float AddMove = 0.3f;

	private const float MoveCoeff = 1f;

	private static readonly List<Vector3> s_Obstacles;

	private static readonly NNConstraint s_NNConstraint;

	private static readonly NNConstraint s_NNConstraintRecastGraph;

	private static Vector3[] s_ReusePointsArray;

	static FreePlaceSelector()
	{
		s_Obstacles = new List<Vector3>();
		s_NNConstraintRecastGraph = new NNConstraint
		{
			graphMask = 1
		};
		s_ReusePointsArray = new Vector3[20];
		s_NNConstraint = NNConstraint.Default;
		s_NNConstraint.constrainArea = true;
		s_NNConstraint.distanceXZ = true;
		s_NNConstraint.graphMask = 1;
	}

	public static void PlaceSpawnPlaces(int count, float radius, Vector3 aroundPoint)
	{
		UpdateArraySizeIfNeeded(count);
		int num = 1;
		int num2 = 1;
		s_ReusePointsArray[0] = new Vector3(aroundPoint.x, aroundPoint.z, radius);
		for (int i = 1; i < count; i++)
		{
			if (num > 0)
			{
				int num3 = num % 4;
				s_ReusePointsArray[i] = s_ReusePointsArray[i - 1] + radius * 2f * new Vector3(num3 switch
				{
					2 => -1, 
					0 => 1, 
					_ => 0, 
				}, num3 switch
				{
					3 => -1, 
					1 => 1, 
					_ => 0, 
				}, 0f);
				num2--;
			}
			if (num2 == 0)
			{
				num++;
				num2 = (num + 1) / 2;
			}
		}
		RelaxPoints(s_ReusePointsArray, aroundPoint, count);
	}

	public static Vector3 GetRelaxedPosition(int index, bool projectOnGround)
	{
		Vector3 result = s_ReusePointsArray[index];
		result.z = result.y;
		result.y = 0f;
		if (projectOnGround)
		{
			Physics.Linecast(new Vector3(result.x, 5000f, result.z), new Vector3(result.x, -5000f, result.z), out var hitInfo, 2359553);
			result = hitInfo.point;
		}
		return result;
	}

	public static void RelaxPoints(Span<Vector3> pointsArray, Span<float> radiusArray, int pointsCount, Vector3? rootPoint = null)
	{
		if (pointsArray.Length > 0)
		{
			if (!rootPoint.HasValue)
			{
				rootPoint = pointsArray[0];
			}
			Span<Vector3> pointsArray2 = stackalloc Vector3[pointsArray.Length];
			for (int i = 0; i < pointsArray.Length; i++)
			{
				Vector3 vector = pointsArray[i];
				float z = radiusArray[i];
				pointsArray2[i] = new Vector3(vector.x, vector.z, z);
			}
			RelaxPoints(pointsArray2, rootPoint.Value, pointsCount);
			for (int j = 0; j < pointsArray.Length; j++)
			{
				Vector3 vector2 = pointsArray2[j];
				pointsArray[j].x = vector2.x;
				pointsArray[j].z = vector2.y;
			}
		}
	}

	private static void RelaxPoints(Span<Vector3> pointsArray, Vector3 rootPoint, int pointCount)
	{
		CollectUnitObstacles(rootPoint, 8f);
		UpdateNavmeshArea(rootPoint);
		int num = 0;
		bool flag = false;
		while (!flag && num++ < 30)
		{
			flag = true;
			for (int i = 0; i < pointCount; i++)
			{
				for (int j = 0; j < s_Obstacles.Count; j++)
				{
					Vector3 obstacle = s_Obstacles[j];
					if (MoveAwayFromObstacle(ref obstacle, ref pointsArray[i]))
					{
						flag = false;
					}
				}
				for (int k = 0; k < pointCount; k++)
				{
					if (i != k && MoveAwayFromObstacle(ref pointsArray[k], ref pointsArray[i], moveObstacle: true))
					{
						flag = false;
					}
				}
				NNInfo nearest = AstarPath.active.GetNearest(new Vector3(pointsArray[i].x, 0f, pointsArray[i].y), s_NNConstraint);
				if (nearest.node != null)
				{
					pointsArray[i].x = nearest.position.x;
					pointsArray[i].y = nearest.position.z;
				}
			}
		}
	}

	private static void CollectUnitObstacles(Vector3 point, float radius)
	{
		s_Obstacles.Clear();
		foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
		{
			if (allUnit.IsInGame && !allUnit.LifeState.IsDead && VectorMath.SqrDistanceXZ(allUnit.Position, point) < radius * radius)
			{
				s_Obstacles.Add(new Vector3(allUnit.Position.x, allUnit.Position.z, allUnit.Corpulence));
			}
		}
	}

	private static void UpdateNavmeshArea(Vector3 point)
	{
		s_NNConstraint.area = (int)AstarPath.active.GetNearest(point, s_NNConstraintRecastGraph).node.Area;
	}

	private static bool MoveAwayFromObstacle(ref Vector3 obstacle, ref Vector3 point, bool moveObstacle = false)
	{
		float num = obstacle.z + point.z;
		Vector2 vector = (Vector2)point - (Vector2)obstacle;
		float num2 = vector.magnitude;
		if (num2 > num)
		{
			return false;
		}
		if (vector == Vector2.zero)
		{
			vector = (PFStatefulRandom.IsUiContext ? UnityEngine.Random.insideUnitCircle.normalized : PFStatefulRandom.View.insideUnitCircle.normalized) * num;
			num2 = num / 2f;
		}
		Vector3 vector2 = (Vector3)(vector * (num - num2 + 0.3f) / num2) * 1f;
		if (moveObstacle)
		{
			vector2 /= 2f;
			obstacle -= vector2;
		}
		point += vector2;
		return true;
	}

	private static void UpdateArraySizeIfNeeded(int size)
	{
		if (s_ReusePointsArray.Length < size)
		{
			s_ReusePointsArray = new Vector3[size];
		}
	}
}
