using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UI.Models.Log.ContextFlag;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("0907b1faea564c23895a4af53acc6c4b")]
public class ReflectIncomingDamageToRandomEnemy : UnitFactComponentDelegate, ITargetRulebookHandler<RuleRollDamage>, IRulebookHandler<RuleRollDamage>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	public bool DisableBattleLog;

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ContextValueModifier MinimumDamage = new ContextValueModifier();

	public ContextValueModifier MaximumDamage = new ContextValueModifier();

	public ContextValueModifier PenetrationMod = new ContextValueModifier();

	public ContextValueModifier PercentDamageModifier = new ContextValueModifier();

	public ContextValueModifier AbsorptionPenetration = new ContextValueModifier();

	public ContextValueModifier DeflectionPenetration = new ContextValueModifier();

	public ContextValueModifier UnmodifiableFlatDamageModifier = new ContextValueModifier();

	public ContextValueModifier UnmodifiablePercentDamageModifier = new ContextValueModifier();

	public ModifierDescriptor ModifierDescriptor;

	public void OnEventAboutToTrigger(RuleRollDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleRollDamage rule)
	{
		if (!Restrictions.IsPassed(base.Fact, rule))
		{
			return;
		}
		DamageData damageData = rule.Damage.Copy();
		if (PercentDamageModifier.Enabled)
		{
			int num = PercentDamageModifier.Calculate(base.Context);
			if (rule.TargetUnit == base.Owner && base.Context.MaybeCaster != null)
			{
				EntityFactManagerComponentsEnumerator<WarhammerMultiplyIncomingDamageBonus> components = base.Context.MaybeCaster.Facts.GetComponents<WarhammerMultiplyIncomingDamageBonus>();
				float num2 = (components.Any() ? (components.Sum((WarhammerMultiplyIncomingDamageBonus p) => p.PercentIncreaseMultiplier - 1f) + 1f) : 1f);
				num = (int)((float)num * num2);
			}
			damageData.Modifiers.Add(ModifierType.PctAdd, num, base.Fact, ModifierDescriptor);
		}
		if (MinimumDamage.Enabled)
		{
			damageData.MinValueModifiers.Add(MinimumDamage.Calculate(base.Context), base.Fact, ModifierDescriptor);
		}
		if (MaximumDamage.Enabled)
		{
			damageData.MaxValueModifiers.Add(MaximumDamage.Calculate(base.Context), base.Fact, ModifierDescriptor);
		}
		if (PenetrationMod.Enabled)
		{
			damageData.Penetration.Add(ModifierType.ValAdd, PenetrationMod.Calculate(base.Context), base.Fact, ModifierDescriptor);
		}
		if (AbsorptionPenetration.Enabled)
		{
			damageData.Absorption.Add(ModifierType.ValAdd, AbsorptionPenetration.Calculate(base.Context), base.Fact, ModifierDescriptor);
		}
		if (DeflectionPenetration.Enabled)
		{
			damageData.Deflection.Add(ModifierType.ValAdd, DeflectionPenetration.Calculate(base.Context), base.Fact, ModifierDescriptor);
		}
		if (UnmodifiableFlatDamageModifier.Enabled)
		{
			damageData.Modifiers.Add(ModifierType.ValAdd_Extra, UnmodifiableFlatDamageModifier.Calculate(base.Context), base.Fact, ModifierDescriptor);
		}
		if (UnmodifiablePercentDamageModifier.Enabled)
		{
			damageData.Modifiers.Add(ModifierType.PctMul_Extra, UnmodifiablePercentDamageModifier.Calculate(base.Context), base.Fact, ModifierDescriptor);
		}
		float num3 = float.MaxValue;
		List<BaseUnitEntity> list = new List<BaseUnitEntity>();
		foreach (UnitGroupMemory.UnitInfo enemy in base.Owner.CombatGroup.Memory.Enemies)
		{
			float num4 = enemy.Unit.DistanceTo(base.Owner);
			if (num4 < num3)
			{
				list.Clear();
				list.Add(enemy.Unit);
				num3 = num4;
			}
			else if (Math.Abs(num4 - num3) < 0.05f)
			{
				list.Add(enemy.Unit);
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		using (ContextData<GameLogDisabled>.RequestIf(DisableBattleLog))
		{
			Rulebook.Trigger(new RuleDealDamage(rule.Target, list.Random(PFStatefulRandom.UnitRandom), damageData)
			{
				DisableGameLog = DisableBattleLog
			});
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
