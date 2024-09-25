using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

public class UnitAnimationSetSpecialAttacks : ScriptableObject, ISerializationCallbackReceiver
{
	[SerializeField]
	[HideInInspector]
	private List<UnitAnimationActionSpecialAttack> m_Actions = new List<UnitAnimationActionSpecialAttack>();

	private IReadOnlyDictionary<UnitAnimationSpecialAttackType, UnitAnimationActionSpecialAttack> m_ActionsByType;

	public List<UnitAnimationActionSpecialAttack> Actions => m_Actions;

	public UnitAnimationActionSpecialAttack GetSpecialAttack(UnitAnimationSpecialAttackType type)
	{
		if (m_ActionsByType == null)
		{
			m_ActionsByType = (from v in m_Actions
				group v by v.AttackType).ToDictionary((IGrouping<UnitAnimationSpecialAttackType, UnitAnimationActionSpecialAttack> v) => v.Key, (IGrouping<UnitAnimationSpecialAttackType, UnitAnimationActionSpecialAttack> v) => v.FirstOrDefault());
		}
		if (!m_ActionsByType.TryGetValue(type, out var value))
		{
			return null;
		}
		return value;
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		m_ActionsByType = null;
	}
}
