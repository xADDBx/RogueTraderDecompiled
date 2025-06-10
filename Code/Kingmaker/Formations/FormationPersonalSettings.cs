using System;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Formations;

[Serializable]
public class FormationPersonalSettings
{
	[SerializeField]
	public bool m_UseRandomPositionAroundTarget;

	[SerializeField]
	[HideIf("m_UseRandomPositionAroundTarget")]
	[Tooltip("Offset from main character. Y axis is in line with main character forward direction. X axis is in line with main character right direction.")]
	public Vector2 m_Offset;

	[Tooltip("Follower will always choose position no closer than this from leader")]
	[SerializeField]
	[ShowIf("m_UseRandomPositionAroundTarget")]
	public float m_MinRadius;

	[Tooltip("Follower will always choose position no further than this from leader")]
	[SerializeField]
	[ShowIf("m_UseRandomPositionAroundTarget")]
	public float m_MaxRadius;

	[SerializeField]
	[Tooltip("Look angle spread. Follower look angle will be equal to main character look angle + Random(-LookAngleSpread/2,LookAngleSpread/2).")]
	public float m_LookAngleRandomSpread = 90f;

	[Tooltip("Limit angle for follower to choose to stand around the leader")]
	[SerializeField]
	[ShowIf("m_UseRandomPositionAroundTarget")]
	public bool m_LimitAngleAroundTarget;

	[SerializeField]
	[ShowIf("m_LimitAngleAroundTarget")]
	[Tooltip("Stand angle spread. Follower will take position behind lead in angle spread Random(-LookAngleSpread/2,LookAngleSpread/2).")]
	public float m_AngleLimit = 90f;

	[SerializeField]
	[Tooltip("Distance between current follower position and his target position on which follower remains idle.")]
	public float m_RepathDistance = 4f;

	public Vector2 GetOffset(int index, AbstractUnitEntity unit)
	{
		if (!m_UseRandomPositionAroundTarget)
		{
			return m_Offset;
		}
		float num = unit.Random.Range(m_MinRadius, m_MaxRadius);
		float num2 = (m_LimitAngleAroundTarget ? (m_AngleLimit / 2f) : 180f);
		float num3 = unit.Random.Range(0f - num2, num2);
		return Quaternion.Euler(Vector3.forward * num3) * Vector2.up * num;
	}
}
