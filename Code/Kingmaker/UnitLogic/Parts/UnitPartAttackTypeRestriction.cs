using Kingmaker.RuleSystem.Enum;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartAttackTypeRestriction : BaseUnitPart, IHashable
{
	[JsonProperty]
	private sbyte[] m_DisabledTypes;

	[JsonProperty]
	private AttackTypeFlag m_DisabledTypesMask;

	public UnitPartAttackTypeRestriction()
	{
		m_DisabledTypes = new sbyte[EnumUtils.GetMaxValue<AttackType>()];
	}

	public void DisableAttackTypes(AttackTypeFlag attackTypes)
	{
		for (int i = 0; i < m_DisabledTypes.Length; i++)
		{
			if (attackTypes.Contains((AttackType)i))
			{
				m_DisabledTypes[i]++;
			}
		}
		RecalculateMask();
	}

	public void EnableAttackTypes(AttackTypeFlag attackTypes)
	{
		for (int i = 0; i < m_DisabledTypes.Length; i++)
		{
			if (attackTypes.Contains((AttackType)i) && m_DisabledTypes[i] > 0)
			{
				m_DisabledTypes[i]--;
			}
		}
		RecalculateMask();
	}

	public bool CanAttack(AttackType attackType)
	{
		return !m_DisabledTypesMask.Contains(attackType);
	}

	private void RecalculateMask()
	{
		m_DisabledTypesMask = (AttackTypeFlag)0;
		for (int i = 0; i < m_DisabledTypes.Length; i++)
		{
			if (m_DisabledTypes[i] > 0)
			{
				m_DisabledTypesMask |= ((AttackType)i).ToFlag();
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(m_DisabledTypes);
		result.Append(ref m_DisabledTypesMask);
		return result;
	}
}
