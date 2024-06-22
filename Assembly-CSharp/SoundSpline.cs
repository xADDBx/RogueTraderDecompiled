using UnityEngine;

public class SoundSpline : MonoBehaviour
{
	private Vector3[] splinePoint;

	private int splineCount;

	public bool DebugDrawSpline = true;

	public bool ShouldUpdatePosition = true;

	private void Start()
	{
		splineCount = base.transform.childCount;
		splinePoint = new Vector3[splineCount];
		for (int i = 0; i < splineCount; i++)
		{
			splinePoint[i] = base.transform.GetChild(i).position;
		}
	}

	private void Update()
	{
		if (ShouldUpdatePosition)
		{
			UpdateSplinePosition();
		}
		if (splineCount > 1)
		{
			for (int i = 0; i < splineCount - 1; i++)
			{
				Debug.DrawLine(splinePoint[i], splinePoint[i + 1], Color.green);
			}
		}
	}

	private void UpdateSplinePosition()
	{
		for (int i = 0; i < splineCount; i++)
		{
			splinePoint[i] = base.transform.GetChild(i).position;
		}
	}

	public Vector3 WhereOnSpline(Vector3 pos)
	{
		int closestSplinePoint = GetClosestSplinePoint(pos);
		if (closestSplinePoint == 0)
		{
			return splineSegment(splinePoint[0], splinePoint[1], pos);
		}
		if (closestSplinePoint == splineCount - 1)
		{
			return splineSegment(splinePoint[splineCount - 1], splinePoint[splineCount - 2], pos);
		}
		Vector3 vector = splineSegment(splinePoint[closestSplinePoint - 1], splinePoint[closestSplinePoint], pos);
		Vector3 vector2 = splineSegment(splinePoint[closestSplinePoint + 1], splinePoint[closestSplinePoint], pos);
		if ((pos - vector).sqrMagnitude <= (pos - vector2).sqrMagnitude)
		{
			return vector;
		}
		return vector2;
	}

	private int GetClosestSplinePoint(Vector3 pos)
	{
		int result = -1;
		float num = 0f;
		for (int i = 0; i < splineCount; i++)
		{
			float sqrMagnitude = (splinePoint[i] - pos).sqrMagnitude;
			if (num == 0f || sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = i;
			}
		}
		return result;
	}

	public Vector3 splineSegment(Vector3 v1, Vector3 v2, Vector3 pos)
	{
		Vector3 rhs = pos - v1;
		Vector3 normalized = (v2 - v1).normalized;
		float num = Vector3.Dot(normalized, rhs);
		if (num < 0f)
		{
			return v1;
		}
		if (num * num > (v2 - v1).sqrMagnitude)
		{
			return v2;
		}
		Vector3 vector = normalized * num;
		return v1 + vector;
	}
}
