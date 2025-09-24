using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.Settings;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.View.Covers;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateDodgeChance : RulebookOptionalTargetEvent<UnitEntity, MechanicEntity>
{
	public readonly ValueModifiersManager DodgeValueModifiers = new ValueModifiersManager();

	public readonly PercentsModifiersManager DodgePercentModifiers = new PercentsModifiersManager();

	public readonly PercentsMultipliersManager DodgePercentMultiplierModifier = new PercentsMultipliersManager();

	public readonly FlagModifiersManager AutoDodgeFlagModifiers = new FlagModifiersManager();

	public readonly ValueModifiersManager MinimumDodgeValueModifier = new ValueModifiersManager();

	public FlagModifiersManager AutoDodgeModifiers = new FlagModifiersManager();

	public FlagModifiersManager NeverDodgeModifiers = new FlagModifiersManager();

	public List<StatType> AgilityReplacementStats = new List<StatType>();

	public const int MaxValueCap = 95;

	private int? m_OverrideDodgeArmorPercentPenalty;

	public CompositeModifiersManager WeaponDodgePenetrationModifiers;

	[CanBeNull]
	public AbilityData Ability { get; }

	public LosCalculations.CoverType CoverType { get; }

	public int BurstIndex { get; }

	public int BaseValue { get; } = 30;


	public int Result { get; private set; }

	public int UncappedResult { get; private set; }

	public int UncappedNegativesCount { get; private set; }

	public bool IsAutoDodge { get; private set; }

	public bool IsNeverDodge { get; private set; }

	[CanBeNull]
	public MechanicEntity MaybeAttacker => base.MaybeTarget;

	[NotNull]
	public UnitEntity Defender => base.Initiator;

	public new NotImplementedException Initiator
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public new NotImplementedException MaybeTarget
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public IEnumerable<Modifier> AllModifiersList
	{
		get
		{
			foreach (Modifier item in DodgeValueModifiers.List)
			{
				yield return item;
			}
			foreach (Modifier item2 in DodgePercentModifiers.List)
			{
				yield return item2;
			}
			foreach (Modifier item3 in AutoDodgeFlagModifiers.List)
			{
				yield return item3;
			}
			foreach (Modifier item4 in MinimumDodgeValueModifier.List)
			{
				yield return item4;
			}
		}
	}

	public int RawResult { get; private set; }

	public override IMechanicEntity GetRuleTarget()
	{
		return MaybeAttacker;
	}

	public RuleCalculateDodgeChance([NotNull] UnitEntity defender, [CanBeNull] MechanicEntity attacker = null, [CanBeNull] AbilityData ability = null, LosCalculations.CoverType coverType = LosCalculations.CoverType.None, int burstIndex = 0)
		: base(defender, attacker)
	{
		Ability = ability;
		CoverType = coverType;
		BurstIndex = burstIndex;
		base.HasNoTarget = attacker == null;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		int num = CalculateDodgeArmorPercentPenalty(Defender, m_OverrideDodgeArmorPercentPenalty);
		StatType statType = StatType.WarhammerAgility;
		int modifiedValue = Defender.Stats.GetStat(statType).ModifiedValue;
		foreach (StatType agilityReplacementStat in AgilityReplacementStats)
		{
			if (Defender.Stats.GetStat(agilityReplacementStat).ModifiedValue > modifiedValue)
			{
				statType = agilityReplacementStat;
				modifiedValue = Defender.Stats.GetStat(agilityReplacementStat).ModifiedValue;
			}
		}
		if (modifiedValue > 0)
		{
			DodgeValueModifiers.Add(modifiedValue, this, statType);
		}
		int valueOrDefault = (MaybeAttacker?.GetAttributeOptional(StatType.WarhammerPerception)?.ModifiedValue).GetValueOrDefault();
		if (valueOrDefault > 0)
		{
			DodgeValueModifiers.Add(-valueOrDefault, this, ModifierDescriptor.AttackerPerception);
		}
		if (num > 0)
		{
			DodgePercentModifiers.Add(-num, Defender.Body.Armor.MaybeArmor, ModifierDescriptor.ArmorPenalty);
		}
		BlueprintWarhammerRoot warhammerRoot = Game.Instance.BlueprintRoot.WarhammerRoot;
		int num2 = BurstIndex * warhammerRoot.CombatRoot.BurstNextBulletDodgePenalty;
		if (num2 > 0)
		{
			DodgeValueModifiers.Add(-num2, this, ModifierDescriptor.BurstFirePenalty);
		}
		CompositeModifiersManager compositeModifiersManager = Ability?.GetWeaponStats().DodgePenetrationModifiers;
		int num3 = compositeModifiersManager?.Value ?? 0;
		if (num3 != 0)
		{
			WeaponDodgePenetrationModifiers = new CompositeModifiersManager();
			WeaponDodgePenetrationModifiers.CopyFrom(compositeModifiersManager);
			DodgeValueModifiers.Add(-num3, this, ModifierDescriptor.Weapon);
		}
		int num4 = TryAddAgilityBasedDodgePenetration();
		if (num4 != 0)
		{
			DodgeValueModifiers.Add(-num4, this, ModifierDescriptor.AttackerAgility);
		}
		if (Defender.IsPlayerEnemy)
		{
			float num5 = SettingsHelper.CalculateCRModifier(SettingsRoot.Difficulty.EnemyDodgePercentModifier);
			DodgeValueModifiers.Add((int)((float)(int)SettingsRoot.Difficulty.EnemyDodgePercentModifier * num5), this, ModifierDescriptor.Difficulty);
		}
		int num6 = BaseValue + DodgeValueModifiers.Value;
		float value = DodgePercentModifiers.Value;
		int num7 = (int)((float)num6 * value);
		if (!DodgePercentMultiplierModifier.Empty)
		{
			num7 = (int)((float)num7 * DodgePercentMultiplierModifier.Value);
		}
		UncappedResult = num7;
		UncappedNegativesCount = ((num6 > 0) ? num7 : num6);
		RawResult = num7;
		if (num7 < MinimumDodgeValueModifier.Value)
		{
			num7 = MinimumDodgeValueModifier.Value;
		}
		Result = Math.Clamp(num7, 0, 95);
		SpecialOverrideWithFeatures();
	}

	public static int CalculateDodgeArmorPercentPenalty(UnitEntity defender, int? overrideDodgeArmorPercentPenalty, BlueprintItemArmor armor = null)
	{
		WarhammerArmorCategory warhammerArmorCategory = armor?.Category ?? defender.Body.Armor.MaybeArmor?.Blueprint.Category ?? WarhammerArmorCategory.None;
		BlueprintArmorType blueprintArmorType = armor?.Type ?? defender.Body.Armor.MaybeArmor?.Blueprint.Type;
		return warhammerArmorCategory switch
		{
			WarhammerArmorCategory.Power => (!defender.GetMechanicFeature(MechanicsFeatureType.IgnorePowerArmourDodgePenalty).Value) ? (overrideDodgeArmorPercentPenalty ?? blueprintArmorType?.DodgeArmorPercentPenalty ?? 25) : 0, 
			WarhammerArmorCategory.Heavy => overrideDodgeArmorPercentPenalty ?? blueprintArmorType?.DodgeArmorPercentPenalty ?? 50, 
			WarhammerArmorCategory.Medium => (!defender.GetMechanicFeature(MechanicsFeatureType.IgnoreMediumArmourDodgePenalty).Value) ? (overrideDodgeArmorPercentPenalty ?? blueprintArmorType?.DodgeArmorPercentPenalty ?? 25) : 0, 
			_ => 0, 
		};
	}

	private int TryAddAgilityBasedDodgePenetration()
	{
		AbilityData ability = Ability;
		int num;
		if ((object)ability == null || !ability.IsMelee)
		{
			num = 0;
		}
		else
		{
			ModifiableValue modifiableValue = MaybeAttacker?.GetStatOptional(StatType.WarhammerAgility);
			num = ((modifiableValue != null) ? ((int)modifiableValue) : 0);
		}
		return num / 2;
	}

	private void SpecialOverrideWithFeatures()
	{
		if ((bool)Defender.Features.AutoDodge || AutoDodgeFlagModifiers.Value || ((bool)Defender.Features.AutoDodgeFriendlyFire && Defender.IsAlly(MaybeAttacker)) || AutoDodgeModifiers.Value)
		{
			IsAutoDodge = true;
			Result = 100;
		}
		else if (MaybeAttacker != null && ((bool)MaybeAttacker.Features.AutoHit || NeverDodgeModifiers.Value))
		{
			IsNeverDodge = true;
			Result = 0;
		}
	}

	public void SetOverrideDodgeArmorPercentPenalty(int dodgeArmorPercentPenalty)
	{
		m_OverrideDodgeArmorPercentPenalty = dodgeArmorPercentPenalty;
	}

	public static int CalculateChapterSpecificDodgeValue(UnitEntity unit, ItemEntityArmor armor)
	{
		int num = Game.Instance.CurrentlyLoadedArea?.GetCR() ?? 0;
		RuleCalculateDodgeChance ruleCalculateDodgeChance = Rulebook.Trigger(new RuleCalculateDodgeChance(unit));
		int num2 = CalculateDodgeArmorPercentPenalty(unit, ruleCalculateDodgeChance.m_OverrideDodgeArmorPercentPenalty);
		int num3 = CalculateDodgeArmorPercentPenalty(unit, ruleCalculateDodgeChance.m_OverrideDodgeArmorPercentPenalty, armor?.Blueprint);
		int num4 = ruleCalculateDodgeChance.Result * 100 / (100 - num2) * (100 - num3) / 100;
		int num5 = ((num > 43) ? 70 : ((num > 31) ? 65 : ((num > 28) ? 55 : ((num > 15) ? 45 : ((num > 2) ? 30 : 25)))));
		return Math.Max(num4 - num5, 0);
	}
}
