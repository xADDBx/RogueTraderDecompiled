using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("599e0ccccecb495469f5898591a28e1c")]
public class WarhammerAbilityCostChangeContextValue : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityActionPointCost>, IRulebookHandler<RuleCalculateAbilityActionPointCost>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public ContextValue Value;

	[SerializeField]
	private BlueprintAbilityReference m_Ability;

	public BlueprintAbility Ability => m_Ability?.Get();

	public void OnEventAboutToTrigger(RuleCalculateAbilityActionPointCost evt)
	{
		if (evt.Blueprint == Ability)
		{
			evt.CostBonus += Value.Calculate(base.Context);
		}
	}

	public void OnEventDidTrigger(RuleCalculateAbilityActionPointCost evt)
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
