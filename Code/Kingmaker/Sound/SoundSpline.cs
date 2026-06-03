using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Sound;

public class SoundSpline : MonoBehaviour
{
	private Vector3[] m_SplinePoint;

	private int m_SplineCount;

	public bool ShouldUpdatePosition = true;

	private void Awake()
	{
		UpdateSplinePoints();
	}

	private void Start()
	{
		UpdateSplinePoints();
	}

	private void Update()
	{
		if (ShouldUpdatePosition)
		{
			UpdateSplinePoints();
		}
	}

	private void OnTransformChildrenChanged()
	{
		UpdateSplinePoints();
	}

	private void UpdateSplinePoints()
	{
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			if (child.gameObject.name != "Sound")
			{
				list.Add(child.position);
			}
		}
		m_SplineCount = list.Count;
		m_SplinePoint = list.ToArray();
	}

	public Vector3 WhereOnSpline(Vector3 pos)
	{
		if (m_SplineCount < 2)
		{
			return base.transform.position;
		}
		int closestSplinePoint = GetClosestSplinePoint(pos);
		if (closestSplinePoint == 0)
		{
			return splineSegment(m_SplinePoint[0], m_SplinePoint[1], pos);
		}
		if (closestSplinePoint == m_SplineCount - 1)
		{
			return splineSegment(m_SplinePoint[m_SplineCount - 1], m_SplinePoint[m_SplineCount - 2], pos);
		}
		Vector3 vector = splineSegment(m_SplinePoint[closestSplinePoint - 1], m_SplinePoint[closestSplinePoint], pos);
		Vector3 vector2 = splineSegment(m_SplinePoint[closestSplinePoint + 1], m_SplinePoint[closestSplinePoint], pos);
		if ((pos - vector).sqrMagnitude <= (pos - vector2).sqrMagnitude)
		{
			return vector;
		}
		return vector2;
	}

	private int GetClosestSplinePoint(Vector3 pos)
	{
		int result = -1;
		float num = float.MaxValue;
		for (int i = 0; i < m_SplineCount; i++)
		{
			float sqrMagnitude = (m_SplinePoint[i] - pos).sqrMagnitude;
			if (sqrMagnitude < num)
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
