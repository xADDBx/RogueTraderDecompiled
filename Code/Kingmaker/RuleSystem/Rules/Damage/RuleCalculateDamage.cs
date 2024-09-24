using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Mechanics.Damage;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Mechanics.Facts;

namespace Kingmaker.RuleSystem.Rules.Damage;

public class RuleCalculateDamage : RulebookOptionalTargetEvent, IDamageHolderRule
{
	private readonly DamageData m_DamageModifiersHolder;

	private readonly bool m_ForceCrit;

	[CanBeNull]
	public readonly RulePerformAttackRoll RollPerformAttackRule;

	public (ContextValueModifierWithType Modifier, MechanicEntityFact Fact, ModifierDescriptor Descriptor) AoeMissDamageModifier;

	public readonly int DistanceToTarget;

	[CanBeNull]
	private DamageData m_CalculatedOverpenetrationDamageData;

	private bool m_CalculatedOverpenetration;

	private bool m_DoNotUseCrModifier;

	public CompositeModifiersManager ValueModifiers => m_DamageModifiersHolder.Modifiers;

	public ValueModifiersManager MinValueModifiers => m_DamageModifiersHolder.MinValueModifiers;

	public ValueModifiersManager MaxValueModifiers => m_DamageModifiersHolder.MaxValueModifiers;

	public CompositeModifiersManager CriticalDamageModifiers => m_DamageModifiersHolder.CriticalDamageModifiers;

	public CompositeModifiersManager PureCriticalDamageModifiers => m_DamageModifiersHolder.PureCriticalDamageModifiers;

	public CompositeModifiersManager Deflection => m_DamageModifiersHolder.Deflection;

	public CompositeModifiersManager Absorption => m_DamageModifiersHolder.Absorption;

	public CompositeModifiersManager Penetration => m_DamageModifiersHolder.Penetration;

	[NotNull]
	private RuleCalculateStatsWeapon InitiatorWeaponStatsRule { get; }

	[CanBeNull]
	public RuleCalculateStatsArmor TargetArmorStatsRule { get; }

	public DamageType? OverrideDamageType { get; set; }

	public DamageType? CheckDamageType { get; set; }

	public DamageData ResultDamage { get; private set; }

	DamageData IDamageHolderRule.Damage => ResultDamage ?? InitiatorWeaponStatsRule.ResultDamage ?? InitiatorWeaponStatsRule.BaseDamage;

	[CanBeNull]
	public AbilityData Ability => InitiatorWeaponStatsRule.Ability;

	public DamageType DamageType => InitiatorWeaponStatsRule.BaseDamage.Type;

	private RuleCalculateDamage([NotNull] MechanicEntity initiator, [CanBeNull] MechanicEntity target, [CanBeNull] AbilityData ability, [CanBeNull] RulePerformAttackRoll performAttackRoll = null, [CanBeNull] DamageData baseDamageOverride = null, [CanBeNull] int? basePenetrationOverride = null, [CanBeNull] int? distance = null, bool forceCrit = false, bool calculatedOverpenetration = false, bool doNotUseCrModifier = false)
		: base(initiator, target)
	{
		InitiatorWeaponStatsRule = ((baseDamageOverride == null && !basePenetrationOverride.HasValue) ? WeaponStatsHelper.GetWeaponStats(ability, ability?.Weapon, (MechanicEntity)base.Initiator, MaybeTarget) : new RuleCalculateStatsWeapon(initiator, target, ability, baseDamageOverride, basePenetrationOverride));
		TargetArmorStatsRule = ((MaybeTarget != null) ? new RuleCalculateStatsArmor(MaybeTarget) : null);
		RollPerformAttackRule = performAttackRoll;
		m_ForceCrit = forceCrit;
		m_CalculatedOverpenetrationDamageData = (calculatedOverpenetration ? baseDamageOverride : null);
		m_CalculatedOverpenetration = calculatedOverpenetration;
		m_DoNotUseCrModifier = doNotUseCrModifier;
		m_DamageModifiersHolder = new DamageData(DamageType.Direct, 0);
		DistanceToTarget = distance.GetValueOrDefault();
	}

	private RuleCalculateDamage(CalculateDamageParams @params)
		: this(@params.Initiator, @params.Target, @params.Ability, @params.PerformAttackRoll, @params.BaseDamageOverride, @params.BasePenetrationOverride, @params.Distance, @params.ForceCrit, @params.CalculatedOverpenetration, @params.DoNotUseCrModifier)
	{
		base.FakeRule = @params.FakeRule;
		base.HasNoTarget = @params.HasNoTarget;
		base.Reason = @params.Reason.GetValueOrDefault();
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Rulebook.Trigger(InitiatorWeaponStatsRule);
		if (TargetArmorStatsRule != null)
		{
			Rulebook.Trigger(TargetArmorStatsRule);
		}
		DamageData damageData = ((m_CalculatedOverpenetration && m_CalculatedOverpenetrationDamageData != null) ? m_CalculatedOverpenetrationDamageData : InitiatorWeaponStatsRule.ResultDamage.Copy());
		damageData.CopyModifiersFrom(m_DamageModifiersHolder);
		if (OverrideDamageType.HasValue && (!CheckDamageType.HasValue || CheckDamageType.Value == damageData.Type))
		{
			damageData = damageData.Copy(OverrideDamageType.Value);
		}
		MechanicEntity maybeTarget = MaybeTarget;
		if (maybeTarget != null && maybeTarget.IsInPlayerParty && !m_DoNotUseCrModifier)
		{
			float num = SettingsHelper.CalculateCRModifier(SettingsRoot.Difficulty.PartyDamageDealtAfterArmorReductionPercentModifier);
			int num2 = (int)((float)(int)SettingsRoot.Difficulty.PartyDamageDealtAfterArmorReductionPercentModifier * num);
			if (num2 != 0)
			{
				damageData.Modifiers.Add(ModifierType.PctMul_Extra, 100 + num2, this, ModifierDescriptor.Difficulty);
			}
		}
		if (RollPerformAttackRule != null && (RollPerformAttackRule.ResultIsRighteousFury || m_ForceCrit))
		{
			damageData.CriticalDamageModifiers.Add(ModifierType.PctAdd, 50, RollPerformAttackRule, ModifierDescriptor.RighteousFury);
			if (RollPerformAttackRule.RighteousFuryAmount > 1f)
			{
				List<Modifier> list = new List<Modifier>();
				foreach (Modifier percentModifiers in damageData.CriticalDamageModifiers.PercentModifiersList)
				{
					list.Add(percentModifiers);
				}
				foreach (Modifier item in list)
				{
					damageData.CriticalDamageModifiers.Add(ModifierType.PctAdd, (int)((float)item.Value * (RollPerformAttackRule.RighteousFuryAmount - 1f)), item.Fact, item.Descriptor);
				}
			}
			damageData.IsCritical = true;
		}
		else if (m_ForceCrit)
		{
			damageData.CriticalDamageModifiers.Add(ModifierType.PctAdd, 50, this, ModifierDescriptor.RighteousFury);
			damageData.IsCritical = true;
		}
		if (RollPerformAttackRule != null && RollPerformAttackRule.ResultDamageIsHalvedBecauseOfAoEMiss)
		{
			damageData.Modifiers.Add(ModifierType.PctMul_Extra, 50, RollPerformAttackRule, ModifierDescriptor.AreaOfEffectAbilityMiss);
			AoeMissDamageModifier.Modifier?.TryApply(damageData.Modifiers, AoeMissDamageModifier.Fact, AoeMissDamageModifier.Descriptor);
		}
		if (TargetArmorStatsRule != null)
		{
			damageData.Absorption.Add(ModifierType.ValAdd, TargetArmorStatsRule.ResultBaseAbsorption, this, ModifierDescriptor.ArmorAbsorption);
			damageData.Absorption.CopyFrom(TargetArmorStatsRule.AbsorptionCompositeModifiers);
			damageData.Deflection.Add(ModifierType.ValAdd, TargetArmorStatsRule.ResultBaseDeflection, this, ModifierDescriptor.ArmorDeflection);
			damageData.Deflection.CopyFrom(TargetArmorStatsRule.DeflectionCompositeModifiers);
		}
		if (damageData.Type.GetInfo().IgnoreArmor)
		{
			damageData.Absorption.Add(ModifierType.PctMul_Extra, 0, this, ModifierDescriptor.Weapon);
			damageData.Deflection.Add(ModifierType.PctMul_Extra, 0, this, ModifierDescriptor.Weapon);
		}
		damageData.MarkCalculated();
		ResultDamage = damageData;
	}

	public static RuleCalculateDamage Trigger(CalculateDamageParams @params)
	{
		RuleCalculateDamage ruleCalculateDamage = CalculateDamageCache.Get(@params);
		if (ruleCalculateDamage == null)
		{
			ruleCalculateDamage = Rulebook.Trigger(new RuleCalculateDamage(@params));
			CalculateDamageCache.Set(@params, ruleCalculateDamage);
		}
		return ruleCalculateDamage;
	}
}
