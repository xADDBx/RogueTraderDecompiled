using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Enum;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.ProjectileAttack;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateStatsWeapon : RulebookOptionalTargetEvent
{
	public readonly DamageData BaseDamage;

	public readonly CompositeModifiersManager RecoilModifiers = new CompositeModifiersManager();

	public readonly CompositeModifiersManager DamageBonusAttributeModifiers = new CompositeModifiersManager();

	public readonly CompositeModifiersManager AdditionalHitChanceModifiers = new CompositeModifiersManager();

	public readonly CompositeModifiersManager DodgePenetrationModifiers = new CompositeModifiersManager();

	public readonly CompositeModifiersManager MaxDistanceModifiers = new CompositeModifiersManager(1);

	public readonly CompositeModifiersManager OptimalDistanceModifiers = new CompositeModifiersManager();

	public readonly CompositeModifiersManager RateOfFireModifiers = new CompositeModifiersManager(1);

	public List<StatType> MeleeDamageStats = new List<StatType> { StatType.WarhammerStrength };

	public List<StatType> RangedAreaDamageStats = new List<StatType> { StatType.WarhammerIntelligence };

	public int? DamageMinOverride { get; set; }

	public int? DamageMaxOverride { get; set; }

	[CanBeNull]
	public AbilityData Ability { get; }

	[CanBeNull]
	public ItemEntityWeapon Weapon { get; }

	public StatType? DamageBonusAttribute { get; private set; }

	public DamageData ResultDamage { get; private set; }

	public int ResultRecoil => RecoilModifiers.Value;

	public int ResultAdditionalHitChance => AdditionalHitChanceModifiers.Value;

	public int ResultDodgePenetration => DodgePenetrationModifiers.Value;

	public int ResultMaxDistance => MaxDistanceModifiers.Value;

	public int ResultOptimalDistance { get; private set; }

	public int ResultRateOfFire => RateOfFireModifiers.Value;

	public StatType MeleeDamageStat => MeleeDamageStats.MaxBy((StatType p) => (base.InitiatorUnit?.Stats.GetStat(p)?.ModifiedValue).GetValueOrDefault());

	public StatType RangedAreaDamageStat => RangedAreaDamageStats.MaxBy((StatType p) => (base.InitiatorUnit?.Stats.GetStat(p)?.ModifiedValue).GetValueOrDefault());

	public RuleCalculateStatsWeapon([NotNull] MechanicEntity initiator, [CanBeNull] MechanicEntity target, [CanBeNull] AbilityData ability, [CanBeNull] DamageData baseDamageOverride = null, [CanBeNull] int? basePenetrationOverride = null)
		: this(initiator, target, ability?.Weapon, ability, baseDamageOverride, basePenetrationOverride)
	{
	}

	public RuleCalculateStatsWeapon([NotNull] MechanicEntity initiator, [CanBeNull] MechanicEntity target, [CanBeNull] ItemEntityWeapon weapon, [CanBeNull] AbilityData ability)
		: this(initiator, target, weapon, ability, null, null)
	{
	}

	private RuleCalculateStatsWeapon([NotNull] MechanicEntity initiator, [CanBeNull] MechanicEntity target, [CanBeNull] ItemEntityWeapon weapon, [CanBeNull] AbilityData ability, [CanBeNull] DamageData baseDamageOverride, [CanBeNull] int? basePenetrationOverride)
		: base(initiator, target)
	{
		Ability = ability;
		Weapon = weapon ?? ability?.Weapon;
		base.HasNoTarget = target == null;
		BaseDamage = baseDamageOverride?.CopyWithoutModifiers() ?? new DamageData(weapon?.Blueprint.DamageType.Type ?? GetAbilityDamageType(), weapon?.Blueprint.WarhammerDamage ?? 0, weapon?.Blueprint.WarhammerMaxDamage ?? 0);
		int value = basePenetrationOverride ?? weapon?.Blueprint.WarhammerPenetration ?? 0;
		BaseDamage.Penetration.Add(ModifierType.ValAdd, value, this, ModifierDescriptor.BaseValue);
		BaseDamage.Overpenetrating = baseDamageOverride?.Overpenetrating ?? false;
		if (Weapon != null)
		{
			RecoilModifiers.Add(ModifierType.ValAdd, Weapon.Blueprint.WarhammerRecoil, this, ModifierDescriptor.BaseValue);
			DodgePenetrationModifiers.Add(ModifierType.ValAdd, Weapon.Blueprint.DodgePenetration, this, ModifierDescriptor.BaseValue);
			MaxDistanceModifiers.Add(ModifierType.ValAdd, Weapon.Blueprint.WarhammerMaxDistance, this, ModifierDescriptor.BaseValue);
			RateOfFireModifiers.Add(ModifierType.ValAdd, Weapon.Blueprint.RateOfFire, this, ModifierDescriptor.BaseValue);
		}
	}

	private DamageType GetAbilityDamageType()
	{
		List<Element> list = Ability?.Blueprint.ElementsArray;
		if (list != null)
		{
			foreach (Element item in list)
			{
				if (item is ContextActionDealDamage contextActionDealDamage)
				{
					if (contextActionDealDamage.DamageType != null)
					{
						return contextActionDealDamage.DamageType.Type;
					}
					break;
				}
			}
		}
		return DamageType.Direct;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		TryApplyEnchantmentsManually();
		if (DamageMinOverride.HasValue)
		{
			BaseDamage.MinValueBase = DamageMinOverride.Value;
		}
		if (DamageMaxOverride.HasValue)
		{
			BaseDamage.MaxValueBase = DamageMaxOverride.Value;
		}
		ItemEntityWeapon weapon = Weapon;
		if (weapon != null && weapon.Blueprint.AttackType == AttackType.Melee)
		{
			AbilityData ability = Ability;
			if ((object)ability != null && ability.Blueprint.IsWeaponAbility)
			{
				goto IL_00e8;
			}
		}
		AbilityData ability2 = Ability;
		if ((object)ability2 != null && ability2.Blueprint?.GetComponent<FakeAttackType>()?.CountAsMelee == true)
		{
			goto IL_00e8;
		}
		goto IL_010f;
		IL_00e8:
		if (!(base.Reason.Fact is Buff))
		{
			DamageBonusAttribute = MeleeDamageStat;
		}
		goto IL_010f;
		IL_010f:
		if (Ability != null)
		{
			AbilityData ability3 = Ability;
			if ((object)ability3 != null && ability3.Blueprint.IsWeaponAbility)
			{
				ItemEntityWeapon weapon2 = Weapon;
				if (weapon2 != null && weapon2.Blueprint.AttackType == AttackType.Ranged && Ability.IsAOE && !(Ability.Blueprint.PatternSettings is ScatterPattern) && !(base.Reason.Fact is Buff))
				{
					DamageBonusAttribute = RangedAreaDamageStat;
				}
			}
		}
		StatType valueOrDefault = DamageBonusAttribute.GetValueOrDefault();
		if (valueOrDefault != 0)
		{
			float? num = Weapon?.Blueprint.DamageStatBonusFactor.GetValue();
			if (num.HasValue)
			{
				float valueOrDefault2 = num.GetValueOrDefault();
				if (valueOrDefault2 >= 0f)
				{
					DamageBonusAttributeModifiers.Add(ModifierType.PctMul, (int)(valueOrDefault2 * 100f), this, ModifierDescriptor.Weapon);
				}
			}
			int value = base.ConcreteInitiator.GetAttributeOptional(valueOrDefault)?.WarhammerBonus ?? ((int)base.ConcreteInitiator.GetStatOptional(valueOrDefault) / 10);
			int num2 = DamageBonusAttributeModifiers.Apply(value);
			BaseDamage.Modifiers.Add(ModifierType.PctAdd, num2 * 5, this, valueOrDefault);
		}
		if (Weapon != null)
		{
			AdditionalHitChanceModifiers.Add(ModifierType.ValAdd, Weapon.Blueprint.AdditionalHitChance, this, ModifierDescriptor.Weapon);
		}
		ResultDamage = BaseDamage.Copy();
		int resultMaxDistance = ResultMaxDistance;
		int value2 = ((Weapon != null && Weapon.Blueprint.IsRanged) ? (resultMaxDistance / 2) : resultMaxDistance);
		ResultOptimalDistance = Mathf.Clamp(OptimalDistanceModifiers.Apply(value2), 0, resultMaxDistance);
	}

	private void TryApplyEnchantmentsManually()
	{
		if (Weapon != null && base.Initiator != Weapon.Wielder)
		{
			foreach (ItemEnchantment enchantment in Weapon.Enchantments)
			{
				enchantment.CallComponents(delegate(IInitiatorRulebookHandler<RuleCalculateStatsWeapon> c)
				{
					try
					{
						c.OnEventAboutToTrigger(this);
					}
					catch (Exception ex2)
					{
						PFLog.Default.Exception(ex2);
					}
				});
			}
		}
		if (base.ConcreteInitiator.Buffs.IsSubscribedOnEventBus)
		{
			return;
		}
		foreach (Buff rawFact in base.ConcreteInitiator.Buffs.RawFacts)
		{
			rawFact.CallComponents(delegate(IInitiatorRulebookHandler<RuleCalculateStatsWeapon> c)
			{
				try
				{
					c.OnEventAboutToTrigger(this);
				}
				catch (Exception ex)
				{
					PFLog.Default.Exception(ex);
				}
			});
		}
	}
}
