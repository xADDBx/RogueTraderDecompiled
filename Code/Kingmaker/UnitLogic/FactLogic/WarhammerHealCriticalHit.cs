using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("4e279637fdf740298df3a5b16881ef5c")]
public class WarhammerHealCriticalHit : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleHealDamage>, IRulebookHandler<RuleHealDamage>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	[SerializeField]
	private BlueprintAbilityReference m_BaseAbility;

	public BlueprintAbility BaseAbility => m_BaseAbility;

	public void OnEventAboutToTrigger(RuleHealDamage evt)
	{
		if (!Restrictions.IsPassed(base.Fact, evt, evt.Ability))
		{
			return;
		}
		AbilityData abilityData = base.Owner.Abilities.Enumerable.FindOrDefault((Ability p) => p.Blueprint == BaseAbility)?.Data;
		if (abilityData == null)
		{
			return;
		}
		int bonusCriticalChance = Rulebook.Trigger(new RuleCalculateRighteousFuryChance(base.Owner, null, abilityData)).BonusCriticalChance;
		RuleRollD100 ruleRollD = Rulebook.Trigger(new RuleRollD100(base.Owner));
		if (ruleRollD.Result <= bonusCriticalChance)
		{
			if (evt.CalculateHealRule.UICriticalBonusRoll == null || !evt.CalculateHealRule.UICriticalBonusChance.HasValue)
			{
				evt.CalculateHealRule.UICriticalBonusRoll = ruleRollD;
				evt.CalculateHealRule.UICriticalBonusChance = bonusCriticalChance;
			}
			int num = new CalculateDamageParams(base.Owner, null, abilityData).Trigger().CriticalDamageModifiers.AllModifiersList.Sum((Modifier p) => p.Value) + 50;
			evt.CalculateHealRule.UIPercentCriticalBonuses.Add(new Tuple<int, string>(num, base.Fact.Name));
			evt.CalculateHealRule.UIPercentCriticalBonus += num;
			evt.CalculateHealRule.PercentBonus += num;
		}
	}

	public void OnEventDidTrigger(RuleHealDamage evt)
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
