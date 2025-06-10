using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Damage;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("4977f892de9e4a85a9c65b3cb21cc976")]
public class ReflectAnyDamageToSelf : UnitFactComponentDelegate, IGlobalRulebookHandler<RuleRollDamage>, IRulebookHandler<RuleRollDamage>, ISubscriber, IGlobalRulebookSubscriber, IGlobalRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, IHashable
{
	public bool ReflectDot;

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ContextValueModifier FlatDamageModifier = new ContextValueModifier();

	public ContextValueModifier PercentDamageModifier = new ContextValueModifier();

	public void OnEventAboutToTrigger(RuleRollDamage evt)
	{
		if (!ReflectDot && evt.Reason.Context?.AssociatedBlueprint is BlueprintBuff)
		{
			return;
		}
		bool flag = evt.ReflectFlatDamageModifiers.HasModifier((Modifier o) => o.Fact == base.Fact) || evt.ReflectPercentDamageModifiers.HasModifier((Modifier o) => o.Fact == base.Fact);
		if (!(evt.TargetUnit == null || evt.TargetUnit == base.Owner || flag) && Restrictions.IsPassed(base.Fact, evt))
		{
			if (PercentDamageModifier.Enabled)
			{
				evt.ReflectPercentDamageModifiers.Add(PercentDamageModifier.Calculate(base.Context), base.Fact);
			}
			if (FlatDamageModifier.Enabled)
			{
				evt.ReflectFlatDamageModifiers.Add(FlatDamageModifier.Calculate(base.Context), base.Fact);
			}
		}
	}

	public void OnEventDidTrigger(RuleRollDamage evt)
	{
	}

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		bool flag = evt.RollDamageRule.ReflectFlatDamageModifiers.HasModifier((Modifier o) => o.Fact == base.Fact) || evt.RollDamageRule.ReflectPercentDamageModifiers.HasModifier((Modifier o) => o.Fact == base.Fact);
		if (base.Owner.IsDeadOrUnconscious || evt.RollDamageRule.ResultReflected == 0 || !flag)
		{
			return;
		}
		RuleCalculateStatsArmor ruleCalculateStatsArmor = new RuleCalculateStatsArmor(base.Owner);
		Rulebook.Trigger(ruleCalculateStatsArmor);
		DamageData damageData = evt.Damage.Copy();
		damageData.MarkCalculated();
		damageData.CalculatedValue = evt.RollDamageRule.ResultReflected;
		if (damageData.Absorption.HasModifier((Modifier x) => x.Type == ModifierType.ValAdd))
		{
			damageData.Absorption.RemoveAll((Modifier x) => true);
			damageData.Absorption.Add(ModifierType.ValAdd, ruleCalculateStatsArmor.ResultBaseAbsorption, (RulebookEvent)null, ModifierDescriptor.ArmorAbsorption);
			damageData.Absorption.CopyFrom(ruleCalculateStatsArmor.AbsorptionCompositeModifiers);
		}
		if (damageData.Deflection.HasModifier((Modifier x) => x.Type == ModifierType.ValAdd))
		{
			damageData.Deflection.RemoveAll((Modifier x) => true);
			damageData.Deflection.Add(ModifierType.ValAdd, ruleCalculateStatsArmor.ResultBaseDeflection, (RulebookEvent)null, ModifierDescriptor.ArmorDeflection);
			damageData.Deflection.CopyFrom(ruleCalculateStatsArmor.DeflectionCompositeModifiers);
		}
		RuleRollDamage ruleRollDamage = new RuleRollDamage(evt.ConcreteInitiator, base.Owner, damageData);
		Rulebook.Trigger(ruleRollDamage);
		Rulebook.Trigger(new RuleDealDamage(evt.ConcreteInitiator, base.Owner, ruleRollDamage));
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
