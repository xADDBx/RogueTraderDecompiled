using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("fbdf123fc48f4c8c9070075459002f01")]
public class TwoWeaponFightingNoPenalties : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityActionPointCost>, IRulebookHandler<RuleCalculateAbilityActionPointCost>, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleCalculateAttackPenalty>, IRulebookHandler<RuleCalculateAttackPenalty>, IHashable
{
	[SerializeField]
	private bool m_KeepAttackPenalty;

	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	public void OnEventAboutToTrigger(RuleCalculateAbilityActionPointCost evt)
	{
		evt.NoTwoWeaponFightingPenalty = evt.NoTwoWeaponFightingPenalty || m_Restrictions.IsPassed(base.Fact, evt, evt.AbilityData);
	}

	public void OnEventDidTrigger(RuleCalculateAbilityActionPointCost evt)
	{
	}

	public void OnEventAboutToTrigger(RuleCalculateAttackPenalty evt)
	{
		if (!m_KeepAttackPenalty)
		{
			evt.NoTwoWeaponFightingPenalty = evt.NoTwoWeaponFightingPenalty || m_Restrictions.IsPassed(base.Fact, evt, evt.AbilityData);
		}
	}

	public void OnEventDidTrigger(RuleCalculateAttackPenalty evt)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
