using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual;

public class SnapToTerrain : MonoBehaviour
{
	public class DebugData
	{
		public enum Type
		{
			C,
			X1,
			X2,
			Z1,
			Z2
		}
	}

	private static readonly RaycastHit[] RaycastHits = new RaycastHit[32];

	private const float Height = 10f;

	public Bounds Bounds;

	public bool NoRotationSnap;

	public bool FixParentRotation;

	public float UpShift;

	[NonSerialized]
	private bool m_Applied;

	[NonSerialized]
	private Quaternion m_BaseLocalRotation;

	[NonSerialized]
	private Quaternion m_BaseParentRotation;

	[CanBeNull]
	private Collider[] m_InnerColliders;

	private Vector3? m_Position;

	public static bool RaycastDebugEnabled => false;

	[NotNull]
	public List<RaycastHit> GetDebugData(DebugData.Type type)
	{
		return TempList.Get<RaycastHit>();
	}

	public void LateUpdate()
	{
		if (m_Position == base.transform.position)
		{
			return;
		}
		m_Position = base.transform.position;
		Transform parent = base.transform.parent;
		if (!m_Applied)
		{
			m_Applied = true;
			m_BaseLocalRotation = base.transform.localRotation;
			m_BaseParentRotation = parent.rotation;
		}
		Quaternion quaternion = (FixParentRotation ? m_BaseParentRotation : parent.rotation);
		Quaternion quaternion2 = Quaternion.Euler(0f, quaternion.eulerAngles.y, 0f);
		Vector3 vector = parent.position + Bounds.center + Bounds.extents.y * Vector3.down;
		float x = Bounds.extents.x;
		float z = Bounds.extents.z;
		Vector3 vector2 = Raycast(vector, vector.y, DebugData.Type.C);
		Vector3 vector3 = Raycast(vector + quaternion2 * Vector3.right * x, vector2.y, DebugData.Type.X1);
		Vector3 vector4 = Raycast(vector - quaternion2 * Vector3.right * x, vector2.y, DebugData.Type.X2);
		Vector3 vector5 = Raycast(vector + quaternion2 * Vector3.forward * z, vector2.y, DebugData.Type.Z1);
		Vector3 vector6 = Raycast(vector - quaternion2 * Vector3.forward * z, vector2.y, DebugData.Type.Z2);
		Vector3 vector7 = Vector3.Cross(vector4 - vector3, vector6 - vector5).normalized;
		if (vector7.y < 0f)
		{
			vector7 = -vector7;
		}
		base.transform.position = (vector3 + vector4 + vector5 + vector6) / 4f - Bounds.center - Bounds.extents.y * Vector3.down;
		if (!NoRotationSnap)
		{
			if (vector7.sqrMagnitude < 0.0001f || (Vector3.up - vector7).sqrMagnitude < 0.0001f)
			{
				base.transform.rotation = m_BaseLocalRotation * quaternion;
			}
			else
			{
				base.transform.rotation = Quaternion.FromToRotation(Vector3.up, vector7) * m_BaseLocalRotation * quaternion;
			}
		}
	}

	private Vector3 Raycast(Vector3 point, float backupHeight, DebugData.Type type)
	{
		if (m_InnerColliders == null)
		{
			m_InnerColliders = GetComponentsInChildren<Collider>();
		}
		Vector3 vector = new Vector3(0f, (Math.Abs(UpShift) < 0.0001f) ? 5f : UpShift);
		int num = Physics.RaycastNonAlloc(new Ray(point + vector, Vector3.down), RaycastHits, 10f, 2359553);
		RaycastHit? raycastHit = null;
		for (int i = 0; i < num; i++)
		{
			RaycastHit value = RaycastHits[i];
			if (!m_InnerColliders.HasItem(value.collider) && !(value.normal.y < 0.2f) && (!raycastHit.HasValue || value.distance < raycastHit.Value.distance))
			{
				raycastHit = value;
			}
		}
		if (raycastHit.HasValue)
		{
			return raycastHit.Value.point;
		}
		point.y = backupHeight;
		return point;
	}

	public static void DrawArrow(Vector3 from, Vector3 to, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20f)
	{
		UnityEngine.Debug.DrawLine(from, to, color);
		Vector3 vector = Quaternion.LookRotation(to - from) * Quaternion.Euler(0f, 180f + arrowHeadAngle, 0f) * Vector3.right;
		Vector3 vector2 = Quaternion.LookRotation(to - from) * Quaternion.Euler(0f, 180f - arrowHeadAngle, 0f) * Vector3.right;
		UnityEngine.Debug.DrawRay(to, vector * arrowHeadLength, color);
		UnityEngine.Debug.DrawRay(to, vector2 * arrowHeadLength, color);
	}
}
