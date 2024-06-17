using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.Settings;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("276c921d2c4c446cb44a4272bfc6323d")]
public class MobStatManager : UnitDifficultyModifiersManager, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, IInitiatorRulebookSubscriber, ITargetRulebookHandler<RuleCalculateDodgeChance>, IRulebookHandler<RuleCalculateDodgeChance>, ITargetRulebookSubscriber, ITargetRulebookHandler<RuleCalculateParryChance>, IRulebookHandler<RuleCalculateParryChance>, IHashable
{
	protected override void UpdateModifiers()
	{
		RemoveModifiers();
		if (base.Owner.Blueprint.Army != null)
		{
			float num = SettingsHelper.CalculateCRModifier(SettingsRoot.Difficulty.NPCAttributesBaseValuePercentModifier);
			float num2 = (float)(int)SettingsRoot.Difficulty.NPCAttributesBaseValuePercentModifier * num;
			float resultPercentModifier = 1f + num2 / 100f;
			AddModifier(StatType.WarhammerWeaponSkill, GetFlatModifier(resultPercentModifier, StatType.WarhammerWeaponSkill));
			AddModifier(StatType.WarhammerBallisticSkill, GetFlatModifier(resultPercentModifier, StatType.WarhammerBallisticSkill));
			AddModifier(StatType.WarhammerStrength, GetFlatModifier(resultPercentModifier, StatType.WarhammerStrength));
			AddModifier(StatType.WarhammerToughness, GetFlatModifier(resultPercentModifier, StatType.WarhammerToughness));
			AddModifier(StatType.WarhammerAgility, GetFlatModifier(resultPercentModifier, StatType.WarhammerAgility));
			AddModifier(StatType.WarhammerPerception, GetFlatModifier(resultPercentModifier, StatType.WarhammerPerception));
			AddModifier(StatType.WarhammerIntelligence, GetFlatModifier(resultPercentModifier, StatType.WarhammerIntelligence));
			AddModifier(StatType.WarhammerWillpower, GetFlatModifier(resultPercentModifier, StatType.WarhammerWillpower));
			AddModifier(StatType.WarhammerFellowship, GetFlatModifier(resultPercentModifier, StatType.WarhammerFellowship));
		}
	}

	private int GetFlatModifier(float resultPercentModifier, StatType stat)
	{
		return (int)((float)base.Owner.Blueprint.GetAttributeValue(stat, onlyBase: false) * resultPercentModifier) - base.Owner.Blueprint.GetAttributeValue(stat);
	}

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		GetModifierAndCurrentCR(out var modifier, out var currentCR);
		int difficultyDamageBonus = base.Owner.Blueprint.GetDifficultyDamageBonus(base.Owner.Blueprint.DifficultyType, currentCR);
		int val = base.Owner.Blueprint.GetDifficultyPenetrationBonus(base.Owner.Blueprint.DifficultyType, currentCR) + modifier;
		evt.Penetration.Add(ModifierType.ValAdd, Math.Max(val, 0), base.Fact);
		ItemEntityWeapon itemEntityWeapon = evt.Ability?.Weapon;
		if (itemEntityWeapon != null)
		{
			difficultyDamageBonus = (itemEntityWeapon.Blueprint.IsMelee ? (difficultyDamageBonus / 2) : difficultyDamageBonus);
			evt.MinValueModifiers.Add(difficultyDamageBonus, base.Fact);
			evt.MaxValueModifiers.Add(difficultyDamageBonus, base.Fact);
		}
	}

	public void OnEventDidTrigger(RuleCalculateDamage evt)
	{
	}

	public void OnEventAboutToTrigger(RuleCalculateDodgeChance evt)
	{
		GetModifierAndCurrentCR(out var modifier, out var currentCR);
		int val = base.Owner.Blueprint.GetDifficultyPenetrationBonus(base.Owner.Blueprint.DifficultyType, currentCR) + modifier;
		evt.DodgeValueModifiers.Add(-Math.Max(val, 0), base.Fact);
	}

	public void OnEventDidTrigger(RuleCalculateDodgeChance evt)
	{
	}

	public void OnEventAboutToTrigger(RuleCalculateParryChance evt)
	{
		GetModifierAndCurrentCR(out var modifier, out var currentCR);
		int val = base.Owner.Blueprint.GetDifficultyPenetrationBonus(base.Owner.Blueprint.DifficultyType, currentCR) + modifier;
		evt.ParryValueModifiers.Add(-Math.Max(val, 0), base.Fact);
	}

	public void OnEventDidTrigger(RuleCalculateParryChance evt)
	{
	}

	private static void GetModifierAndCurrentCR(out int modifier, out int currentCR)
	{
		currentCR = Game.Instance.CurrentlyLoadedArea?.GetCR() ?? 0;
		float num = SettingsHelper.CalculateCRModifier(SettingsRoot.Difficulty.EnemyDodgePercentModifier);
		modifier = (int)((float)(int)SettingsRoot.Difficulty.EnemyDodgePercentModifier * num);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
