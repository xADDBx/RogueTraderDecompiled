using System;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Visual;

public class AoeSnapToTerrain : MonoBehaviour
{
	private static readonly RaycastHit[] s_RaycastHits = new RaycastHit[32];

	private bool m_SingleRaycastExecuted;

	private bool m_IsInitialized;

	private Quaternion m_DefaultRot;

	private Vector3 m_DefaultRight;

	private Vector3 m_DefaultForward;

	private Vector3 m_DefaultUp;

	public LayerMask RaycastMask = -1;

	public Bounds Bounds = new Bounds(default(Vector3), Vector3.one);

	public float UpDownShift = 7f;

	public bool SingleRaycast;

	public float LimitAngleXZ = 360f;

	private Collider[] m_InnerColliders;

	private void OnEnable()
	{
		m_SingleRaycastExecuted = false;
	}

	private void LateUpdate()
	{
		if (!m_IsInitialized)
		{
			m_DefaultRot = base.transform.rotation;
			m_DefaultRight = base.transform.right;
			m_DefaultForward = base.transform.forward;
			m_DefaultUp = base.transform.up;
			m_IsInitialized = true;
		}
		if (!SingleRaycast || !m_SingleRaycastExecuted)
		{
			m_SingleRaycastExecuted = true;
			UpdateSnap();
		}
	}

	private void UpdateSnap()
	{
		Vector3 vector = base.transform.position + Bounds.center;
		float x = Bounds.extents.x;
		float z = Bounds.extents.z;
		Vector3 point = vector + m_DefaultRight * x;
		Vector3 point2 = vector - m_DefaultRight * x;
		Vector3 point3 = vector + m_DefaultForward * z;
		Vector3 point4 = vector - m_DefaultForward * z;
		Vector3 vector2 = Raycast(vector, base.transform.position.y + Bounds.center.y);
		point = Raycast(point, vector2.y);
		point2 = Raycast(point2, vector2.y);
		point3 = Raycast(point3, vector2.y);
		point4 = Raycast(point4, vector2.y);
		Vector3 normalized = (point3 - point4).normalized;
		Vector3 vector3 = Vector3.Cross(point - point2, normalized).normalized;
		if (vector3.y < 0f)
		{
			vector3 = -vector3;
		}
		Quaternion a = Quaternion.LookRotation(normalized, vector3);
		float num = Mathf.Acos(Vector3.Dot(m_DefaultUp, vector3)) * 57.29578f;
		float t = 0f;
		if (num > LimitAngleXZ)
		{
			t = (num - LimitAngleXZ) / num;
		}
		base.transform.rotation = Quaternion.Slerp(a, m_DefaultRot, t);
		base.transform.position = (point + point2 + point3 + point4) / 4f;
	}

	private Vector3 Raycast(Vector3 point, float backupHeight)
	{
		if (m_InnerColliders == null)
		{
			m_InnerColliders = GetComponentsInChildren<Collider>();
		}
		float num = ((Math.Abs(UpDownShift) < 0.0001f) ? 7f : UpDownShift);
		Ray ray = new Ray(point + num * Vector3.up, Vector3.down);
		int num2 = Physics.RaycastNonAlloc(ray, s_RaycastHits, num * 2f, RaycastMask);
		float num3 = float.MaxValue;
		Vector3 result = point;
		result.y = backupHeight;
		for (int i = 0; i < num2; i++)
		{
			RaycastHit raycastHit = s_RaycastHits[i];
			if (!m_InnerColliders.HasItem(raycastHit.collider) && !(raycastHit.normal.y < 0.2f))
			{
				float sqrMagnitude = (ray.origin - raycastHit.point).sqrMagnitude;
				if (sqrMagnitude < num3)
				{
					num3 = sqrMagnitude;
					result = raycastHit.point;
				}
			}
		}
		return result;
	}

	private void OnDrawGizmosSelected()
	{
		Color color = Gizmos.color;
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(base.transform.position + Bounds.center, Bounds.size);
		Gizmos.color = color;
	}
}
