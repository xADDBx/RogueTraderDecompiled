using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartAttackOfOpportunityModifier : UnitPart, IHashable
{
	private AttackOfOpportunityModifier m_Component;

	public bool EnableAndPrioritizeRangedAttack => m_Component?.EnableAndPrioritizeRangedAttack ?? false;

	public void Add(UnitFact fact, AttackOfOpportunityModifier component)
	{
		m_Component = component;
	}

	public void Remove(UnitFact fact, AttackOfOpportunityModifier component)
	{
		m_Component = null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
