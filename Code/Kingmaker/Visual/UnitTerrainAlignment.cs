using Kingmaker.Mechanics.Entities;
using Kingmaker.View.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.Visual;

public class UnitTerrainAlignment : MonoBehaviour
{
	public float SampleRadious = 1f;

	private AbstractUnitEntityView m_View;

	private Vector3 p1;

	private Vector3 p2;

	private Vector3 p3;

	private Plane plane;

	[SerializeField]
	[Range(0f, 1f)]
	private float maxTilt = 0.5f;

	public void Start()
	{
		m_View = GetComponentInParent<AbstractUnitEntityView>();
	}

	public void Update()
	{
		if (m_View != null)
		{
			AbstractUnitEntity entityData = m_View.EntityData;
			if (entityData != null && entityData.IsInGame)
			{
				goto IL_002f;
			}
		}
		if (!Application.isEditor)
		{
			return;
		}
		goto IL_002f;
		IL_002f:
		Vector3 forward = base.transform.forward;
		Vector3 right = base.transform.right;
		p1 = base.transform.position + forward * SampleRadious;
		p2 = base.transform.position - forward * SampleRadious + right * SampleRadious;
		p3 = base.transform.position - forward * SampleRadious - right * SampleRadious;
		plane = new Plane(GetTerrainPoint(p1), GetTerrainPoint(p2), GetTerrainPoint(p3));
		Vector3 vector = base.transform.parent.position + base.transform.parent.forward - base.transform.position;
		vector = new Vector3(vector.x, 0f, vector.z);
		float num = Vector3.Dot(vector, plane.normal);
		vector -= num * plane.normal;
		vector.Normalize();
		if (vector != Vector3.zero)
		{
			Quaternion to = Quaternion.LookRotation(vector, Vector3.Lerp(plane.normal, Vector3.up, maxTilt));
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, 200f * Time.deltaTime);
		}
	}

	private float GetAngle(float h1, float h2)
	{
		return Mathf.Atan2(h1 - h2, SampleRadious) * 57.29578f;
	}

	private float GetTerrainHeight(Vector3 p1)
	{
		if (Physics.Linecast(p1 + 4f * base.transform.up, p1 - 4f * base.transform.up, out var hitInfo, 2359553))
		{
			return hitInfo.point.y;
		}
		return p1.y;
	}

	private Vector3 GetTerrainPoint(Vector3 p1)
	{
		if (Physics.Linecast(p1 + 4f * base.transform.up, p1 - 4f * base.transform.up, out var hitInfo, 2359553))
		{
			return hitInfo.point;
		}
		return p1;
	}
}
