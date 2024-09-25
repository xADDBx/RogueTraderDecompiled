using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("7a108a43007fe9a46a8e78f13028c195")]
public class WarhammerModifyWeaponEffectiveRange : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityRange>, IRulebookHandler<RuleCalculateAbilityRange>, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleCalculateAbilityDistanceFactor>, IRulebookHandler<RuleCalculateAbilityDistanceFactor>, IHashable
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public float modifier = 1f;

	public bool extendAbilityRange;

	public void OnEventAboutToTrigger(RuleCalculateAbilityRange evt)
	{
		if (Restrictions.IsPassed(base.Fact, evt, evt.Ability) && (!(base.OwnerBlueprint is BlueprintAbility) || evt.Ability.Blueprint == base.OwnerBlueprint) && extendAbilityRange)
		{
			int defaultRange = evt.DefaultRange;
			evt.Bonus += Mathf.RoundToInt((float)defaultRange * modifier) - defaultRange;
		}
	}

	public void OnEventDidTrigger(RuleCalculateAbilityRange evt)
	{
	}

	public void OnEventAboutToTrigger(RuleCalculateAbilityDistanceFactor evt)
	{
		if (Restrictions.IsPassed(base.Fact, evt, evt.Ability) && (!(base.OwnerBlueprint is BlueprintAbility) || evt.Ability.Blueprint == base.OwnerBlueprint) && evt.Ability.Weapon != null)
		{
			int resultMaxDistance = evt.Ability.Weapon.GetWeaponStats().ResultMaxDistance;
			evt.BonusEffectiveRange += Mathf.RoundToInt((float)resultMaxDistance * modifier) - resultMaxDistance;
		}
	}

	public void OnEventDidTrigger(RuleCalculateAbilityDistanceFactor evt)
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
