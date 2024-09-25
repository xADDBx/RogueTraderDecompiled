using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.View;

internal struct IntersectionResult
{
	public bool HasIntersection;

	public float SqrDistance;

	public Vector3 Point;

	[CanBeNull]
	public UnitMovementAgentBase Unit1;

	[CanBeNull]
	public UnitMovementAgentBase Unit2;

	public Vector3 NavmeshContactPoint;

	public void Update(Vector3 point, float sqrDistance, UnitMovementAgentBase unit1, UnitMovementAgentBase unit2)
	{
		if (!HasIntersection || sqrDistance < SqrDistance)
		{
			HasIntersection = true;
			Point = point;
			SqrDistance = sqrDistance;
			Unit1 = unit1;
			Unit2 = unit2;
		}
	}

	public void Update(Vector3 point, float sqrDistance, UnitMovementAgentBase unit, Vector3 navmeshContactPoint)
	{
		if (!HasIntersection || sqrDistance < SqrDistance)
		{
			HasIntersection = true;
			Point = point;
			SqrDistance = sqrDistance;
			Unit1 = unit;
			NavmeshContactPoint = navmeshContactPoint;
		}
	}

	public Vector3 GetSecondPoint()
	{
		if (!(Unit2 != null))
		{
			return NavmeshContactPoint;
		}
		return Unit2.transform.position;
	}
}
