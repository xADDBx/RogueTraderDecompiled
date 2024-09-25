using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment.UI;

public class SliceGizmo : MonoBehaviour
{
	private Transform[] m_Bones;

	private Matrix4x4[] m_Bindposes;

	private Dictionary<Transform, int> m_BonesMap = new Dictionary<Transform, int>();

	private Dictionary<Transform, Transform> m_ParentMap = new Dictionary<Transform, Transform>();

	private static List<Transform> addedBones = new List<Transform>();

	private Vector3 m_ParentPosition;

	private Vector3 m_SeveredJointPosition;

	private Vector3 m_ChildPosition;

	private Vector3 m_Normal;

	public UnitDismembermentManager.DismembermentBone SliceBone { get; private set; }

	public Vector3 Normal => m_Normal;

	public void Init(UnitDismembermentManager.DismembermentBone sliceBone, Transform[] bones, Matrix4x4[] bindposes, Transform root, GameObject referenceAdjustmentSceleton)
	{
		SliceBone = sliceBone;
		m_Bones = bones;
		m_Bindposes = bindposes;
		m_BonesMap.Clear();
		addedBones.Clear();
		for (int i = 0; i < m_Bones.Length; i++)
		{
			if (!addedBones.Contains(m_Bones[i]))
			{
				m_BonesMap.Add(m_Bones[i], i);
				addedBones.Add(m_Bones[i]);
			}
			else
			{
				UnityEngine.Debug.Log("DC: Double Bones: " + m_Bones[i]);
			}
		}
		m_ParentMap.Clear();
		if (referenceAdjustmentSceleton != null)
		{
			foreach (KeyValuePair<Transform, int> item in m_BonesMap)
			{
				Transform transform = referenceAdjustmentSceleton.transform.FindChildRecursive(item.Key.name);
				if (transform != null && transform.parent != null)
				{
					Transform value = root.FindChildRecursive(transform.parent.name);
					m_ParentMap.Add(item.Key, value);
				}
			}
		}
		else
		{
			foreach (KeyValuePair<Transform, int> item2 in m_BonesMap)
			{
				m_ParentMap.Add(item2.Key, item2.Key.parent);
			}
		}
		Transform transform2 = m_ParentMap[sliceBone.Transform];
		List<int> list = new List<int>();
		foreach (KeyValuePair<Transform, Transform> item3 in m_ParentMap)
		{
			if (item3.Value == sliceBone.Transform && m_BonesMap.TryGetValue(item3.Key, out var value2))
			{
				list.Add(value2);
			}
		}
		Vector3 childPosition = default(Vector3);
		if (list.Count > 0)
		{
			foreach (int item4 in list)
			{
				_ = m_Bindposes[item4].inverse;
				childPosition += m_Bones[item4].position;
			}
			childPosition /= (float)list.Count;
		}
		else
		{
			childPosition = SliceBone.Transform.position;
		}
		m_ParentPosition = transform2.position;
		m_SeveredJointPosition = SliceBone.Transform.position;
		m_ChildPosition = childPosition;
		Update();
	}

	private void Update()
	{
		Vector3 a = m_ParentPosition - m_SeveredJointPosition;
		Vector3 b = m_SeveredJointPosition - m_ChildPosition;
		Vector3 position = Vector3.Lerp(m_SeveredJointPosition, m_ChildPosition, SliceBone.SliceOffset);
		Vector3 vector = -Vector3.Lerp(a, b, SliceBone.SliceOffset).normalized;
		Quaternion quaternion = Quaternion.Euler(SliceBone.SliceOrientationEuler);
		m_Normal = quaternion * vector;
		if (Vector3.Dot(m_Normal, vector) < 0f)
		{
			m_Normal = -m_Normal;
		}
		m_Normal = ClampNormalToBicone(m_Normal, vector, 30f);
		base.transform.position = position;
		base.transform.rotation = Quaternion.FromToRotation(Vector3.forward, m_Normal);
	}

	public static Vector3 ClampNormalToBicone(Vector3 normal, Vector3 axis, float maximumDegrees)
	{
		float num = Mathf.Cos(maximumDegrees * (MathF.PI / 180f));
		float f = Vector3.Dot(normal, axis);
		Vector3 vector = normal;
		if (Mathf.Abs(f) < num)
		{
			float num2 = Mathf.Sign(f);
			float num3 = num - Mathf.Abs(f);
			Vector3 vector2 = axis * num3 * num2;
			float num4 = 1f;
			float num5 = 1f;
			float num6 = 100f;
			for (int num7 = 16; num7 > 0; num7--)
			{
				vector = (normal + vector2 * num4).normalized;
				float num8 = Mathf.Abs(Vector3.Dot(vector, axis));
				if (num8 > num)
				{
					num6 = num4;
					num4 = (num4 + num5) / 2f;
				}
				else if (num8 < num)
				{
					num5 = num4;
					num4 = (num4 + num6) / 2f;
				}
			}
		}
		return vector;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawLine(base.transform.position, base.transform.position + m_Normal);
	}
}
