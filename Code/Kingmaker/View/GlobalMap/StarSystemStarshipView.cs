using System.Collections.Generic;
using Kingmaker.Controllers;
using UnityEngine;

namespace Kingmaker.View.GlobalMap;

[RequireComponent(typeof(CurvedLineRenderer))]
public class StarSystemStarshipView : MonoBehaviour, IUpdatable
{
	[SerializeField]
	private Transform mechanicsTransform;

	private Transform m_VisualTransform;

	private CurvedLineRenderer m_CurvedLineRend;

	public Vector3 Position
	{
		get
		{
			if (!m_VisualTransform)
			{
				return Vector3.zero;
			}
			return m_VisualTransform.position;
		}
	}

	private void Awake()
	{
		m_VisualTransform = base.transform;
		m_CurvedLineRend = GetComponent<CurvedLineRenderer>();
	}

	private void OnEnable()
	{
		m_VisualTransform.SetPositionAndRotation(mechanicsTransform.position, mechanicsTransform.rotation);
		Game.Instance.CustomLateUpdateController.Add(this);
	}

	private void OnDisable()
	{
		Game.Instance.CustomLateUpdateController.Remove(this);
	}

	void IUpdatable.Tick(float delta)
	{
		Vector3 position = mechanicsTransform.position;
		Quaternion rotation = mechanicsTransform.rotation;
		Vector3 position2 = m_VisualTransform.position;
		Quaternion rotation2 = m_VisualTransform.rotation;
		if (!(position == position2) || !(rotation == rotation2))
		{
			float num = delta / (float)RealTimeController.SystemStepTimeSpan.TotalSeconds;
			float maxDistanceDelta = num * Vector3.Distance(position2, position);
			float maxDegreesDelta = num * Quaternion.Angle(rotation2, rotation);
			position2 = Vector3.MoveTowards(position2, position, maxDistanceDelta);
			rotation2 = Quaternion.RotateTowards(rotation2, rotation, maxDegreesDelta);
			m_VisualTransform.SetPositionAndRotation(position2, rotation2);
		}
	}

	public void DrawPath(List<Vector3> path)
	{
		UndrawPath();
		for (int i = 0; i < path.Count; i++)
		{
			GameObject obj = new GameObject();
			obj.name = $"PathPoint {i}";
			obj.AddComponent<CurvedLinePoint>();
			obj.transform.position = path[i];
			obj.transform.parent = base.transform;
			m_CurvedLineRend.ManualUpdate();
		}
		m_CurvedLineRend.ManualUpdate();
	}

	public void UndrawPath()
	{
		CurvedLinePoint[] componentsInChildren = GetComponentsInChildren<CurvedLinePoint>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.DestroyImmediate(componentsInChildren[i].gameObject);
			m_CurvedLineRend.ManualUpdate();
		}
		m_CurvedLineRend.ManualUpdate();
	}
}
