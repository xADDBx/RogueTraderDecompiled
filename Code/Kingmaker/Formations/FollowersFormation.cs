using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.Formations;

[TypeId("fb8f0063239b48cf9687ee900a43d354")]
public class FollowersFormation : BlueprintScriptableObject, IImmutablePartyFormation
{
	[SerializeField]
	[Tooltip("Offset from main character. Y axis is in line with main character forward direction. X axis is in line with main character right direction.")]
	private Vector2 m_PlayerOffset;

	[SerializeField]
	[Tooltip("Followers formation.")]
	private Vector2[] m_Formation;

	[SerializeField]
	[Tooltip("Distance between current follower position and his target position on which follower remains idle.")]
	private float m_RepathDistance = 4f;

	[SerializeField]
	[Tooltip("Repath delay in seconds.")]
	private float m_RepathCooldownSec = 1.25f;

	[SerializeField]
	[Tooltip("Look angle spread. Follower look angle will be equal to main character look angle + Random(-LookAngleSpread/2,LookAngleSpread/2).")]
	private float m_LookAngleRandomSpread = 90f;

	public const float MoveProximity = 0.1f;

	public Vector2 PlayerOffset => m_PlayerOffset;

	public float RepathDistance => m_RepathDistance;

	public float RepathCooldownSec => m_RepathCooldownSec;

	public float LookAngleRandomSpread => m_LookAngleRandomSpread;

	public float Length => 1f;

	public AbstractUnitEntity Tank => null;

	public Vector3 GetPosition(int index)
	{
		int num = m_Formation.Length;
		if (num == 0)
		{
			return Vector3.zero;
		}
		int num2 = index % num;
		Vector2 vector = m_Formation[num2];
		return new Vector3(vector.x, 0f, vector.y);
	}

	public Vector2 GetOffset(int index, AbstractUnitEntity unit)
	{
		Vector3 position = GetPosition(index);
		return new Vector2(position.x, position.z);
	}
}
