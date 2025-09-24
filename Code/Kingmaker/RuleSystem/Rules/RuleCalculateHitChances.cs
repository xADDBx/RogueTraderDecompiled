using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.View.Covers;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateHitChances : RulebookTargetEvent
{
	public readonly ValueModifiersManager HitChanceValueModifiers = new ValueModifiersManager();

	public readonly ValueModifiersManager InitiatorWeaponSkillValueModifiers = new ValueModifiersManager();

	public readonly ValueModifiersManager TargetWeaponSkillValueModifiers = new ValueModifiersManager();

	public FlagModifiersManager AutoHitModifier = new FlagModifiersManager();

	public FlagModifiersManager AutoCritModifier = new FlagModifiersManager();

	public FlagModifiersManager NeverCritMofifier = new FlagModifiersManager();

	private RuleCalculateSuperiority m_RuleCalculateSuperiority;

	public AbilityData Ability { get; }

	public int BurstIndex { get; }

	public Vector3 EffectiveCasterPosition { get; }

	public Vector3 AbilityTargetPosition { get; }

	public RuleCalculateRighteousFuryChance RighteousFuryChanceRule { get; }

	public float OverpenetrationModifier { get; }

	public int ResultHitChance { get; private set; }

	public int ResultHitChanceNoUpperLimit { get; private set; }

	public int ResultBaseChancesBeforeRecoil { get; private set; }

	public LosCalculations.CoverType ResultLos { get; private set; }

	[CanBeNull]
	public MechanicEntity ResultCoverEntity { get; private set; }

	[CanBeNull]
	public RuleCalculateCoverHitChance ResultCoverHitChanceRule { get; private set; }

	public int ResultSuperiorityNumber => m_RuleCalculateSuperiority?.ResultSuperiorityNumber ?? 0;

	public int ResultTargetSuperiorityPenalty => m_RuleCalculateSuperiority?.ResultTargetSuperiorityPenalty ?? 0;

	public int ResultRighteousFurySuperiorityBonus { get; private set; }

	public int ResultRighteousFuryChance => RighteousFuryChanceRule.ResultChance;

	public bool IsMelee
	{
		get
		{
			if (Ability.Weapon == null)
			{
				return Ability.Blueprint.GetRange() == 1;
			}
			return Ability.Weapon.Blueprint.IsMelee;
		}
	}

	public bool IsScatter => Ability.IsScatter;

	public bool IsAOE => Ability.IsAOE;

	public bool IsAutoHit { get; set; }

	public bool IsAutoMiss { get; private set; }

	public int InitiatorBallisticSkill { get; private set; }

	public int BallisticSkillPenalty { get; private set; }

	public int ResultBallisticSkill { get; private set; }

	public float DistanceFactor { get; private set; }

	public int WeaponStatsResultRecoil { get; private set; }

	public int ResultWeaponSkillPenalty { get; private set; }

	public LosCalculations.CoverType? FakeCover { get; private set; }

	public bool IgnoreRealCoverByFakeCover { get; private set; }

	public IEnumerable<Modifier> AllModifiersList
	{
		get
		{
			foreach (Modifier item in HitChanceValueModifiers.List)
			{
				yield return item;
			}
			if (m_RuleCalculateSuperiority != null)
			{
				foreach (Modifier item2 in m_RuleCalculateSuperiority.SuperiorityValueModifiers.List)
				{
					yield return item2;
				}
			}
			foreach (Modifier item3 in InitiatorWeaponSkillValueModifiers.List)
			{
				yield return item3;
			}
			foreach (Modifier item4 in TargetWeaponSkillValueModifiers.List)
			{
				yield return item4;
			}
		}
	}

	public int RawResult { get; private set; }

	public RuleCalculateHitChances([NotNull] IMechanicEntity initiator, [NotNull] IMechanicEntity target, [NotNull] AbilityData ability, int burstIndex, Vector3 effectiveCasterPosition, Vector3 abilityTargetPosition)
		: this((MechanicEntity)initiator, (MechanicEntity)target, ability, burstIndex, effectiveCasterPosition, abilityTargetPosition)
	{
	}

	public RuleCalculateHitChances([NotNull] IMechanicEntity initiator, [NotNull] IMechanicEntity target, [NotNull] AbilityData ability, int burstIndex)
		: this(initiator, target, ability, burstIndex, initiator.Position, target.Position)
	{
	}

	public RuleCalculateHitChances([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [NotNull] AbilityData ability, int burstIndex, Vector3 effectiveCasterPosition, Vector3 abilityTargetPosition, float overpenetrationModifier = 1f)
		: base(initiator, target)
	{
		Ability = ability;
		BurstIndex = burstIndex;
		EffectiveCasterPosition = effectiveCasterPosition;
		AbilityTargetPosition = abilityTargetPosition;
		OverpenetrationModifier = overpenetrationModifier;
		RighteousFuryChanceRule = new RuleCalculateRighteousFuryChance((MechanicEntity)base.Initiator, (MechanicEntity)Target, Ability);
	}

	public RuleCalculateHitChances([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [NotNull] AbilityData ability, int burstIndex)
		: this(initiator, target, ability, burstIndex, initiator.Position, target.Position)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		IAbilityAoEPatternProvider patternSettings = Ability.GetPatternSettings();
		LosDescription losDescription = ((patternSettings != null && patternSettings.CalculateAttackFromPatternCentre) ? LosCalculations.GetWarhammerLos(AbilityTargetPosition, default(IntRect), (MechanicEntity)Target) : LosCalculations.GetWarhammerLos(EffectiveCasterPosition, base.Initiator.SizeRect, (MechanicEntity)Target));
		if ((losDescription.CoverType == LosCalculations.CoverType.None || IgnoreRealCoverByFakeCover) && FakeCover.HasValue)
		{
			losDescription = new LosDescription(FakeCover.Value, losDescription.Obstacle);
		}
		ResultLos = losDescription;
		ResultCoverEntity = losDescription.ObstacleEntity;
		if (IsMelee)
		{
			OnTriggerMelee();
		}
		else
		{
			ResultCoverHitChanceRule = Rulebook.Trigger(new RuleCalculateCoverHitChance((MechanicEntity)base.Initiator, (MechanicEntity)Target, Ability, ResultLos, ResultCoverEntity));
			if (IsScatter)
			{
				OnTriggerScatter();
			}
			else if (IsAOE)
			{
				OnTriggerAOE();
			}
			else
			{
				OnTriggerRanged();
			}
		}
		SpecialOverrideWithFeatures();
		Rulebook.Trigger(RighteousFuryChanceRule);
	}

	private void SpecialOverrideWithFeatures()
	{
		if ((bool)base.ConcreteInitiator.Features.AutoHit || AutoHitModifier.Value)
		{
			IsAutoHit = true;
			RawResult = 100;
			ResultHitChance = 100;
		}
		else if ((bool)base.ConcreteInitiator.Features.AutoMiss)
		{
			IsAutoMiss = true;
			RawResult = 0;
			ResultHitChance = 0;
		}
	}

	private void OnTriggerMelee()
	{
		RawResult = 100;
		ResultHitChance = 100;
		m_RuleCalculateSuperiority = Rulebook.Trigger(new RuleCalculateSuperiority(base.Initiator, Target, Ability));
		StatType statType = Ability.Blueprint.GetComponent<WarhammerOverrideAbilityMeleeStat>()?.Stat ?? StatType.WarhammerWeaponSkill;
		int num = ((MechanicEntity)base.Initiator).GetAttributeOptional(statType)?.ModifiedValue ?? 0;
		ResultWeaponSkillPenalty = Rulebook.Trigger(new RuleCalculateAttackPenalty((MechanicEntity)base.Initiator, Ability)).ResultWeaponSkillPenalty;
		int num2 = Math.Max(num - ResultWeaponSkillPenalty, 0) + InitiatorWeaponSkillValueModifiers.Value;
		int num3 = (((MechanicEntity)Target).GetAttributeOptional(statType)?.ModifiedValue ?? 0) + TargetWeaponSkillValueModifiers.Value;
		ResultRighteousFurySuperiorityBonus = (base.TargetUnit?.Features?.HalfSuperiorityCriticalChance ? (ResultTargetSuperiorityPenalty / 2) : ResultTargetSuperiorityPenalty);
		int num4 = num2 - num3 + ResultRighteousFurySuperiorityBonus;
		if (num4 != 0)
		{
			RighteousFuryChanceRule.ChanceModifiers.Add(num4, this, statType);
		}
	}

	private void OnTriggerScatter()
	{
		RawResult = 100;
		ResultHitChance = 100;
	}

	private void OnTriggerAOE()
	{
		OnTriggerRanged();
	}

	private void OnTriggerRanged()
	{
		RuleCalculateAbilityDistanceFactor ruleCalculateAbilityDistanceFactor = Rulebook.Trigger(new RuleCalculateAbilityDistanceFactor((MechanicEntity)base.Initiator, (MechanicEntity)Target, Ability));
		DistanceFactor = ruleCalculateAbilityDistanceFactor.Result;
		InitiatorBallisticSkill = ((MechanicEntity)base.Initiator).GetAttributeOptional(StatType.WarhammerBallisticSkill)?.ModifiedValue ?? 0;
		RuleCalculateAttackPenalty ruleCalculateAttackPenalty = Rulebook.Trigger(new RuleCalculateAttackPenalty((MechanicEntity)base.Initiator, Ability));
		BallisticSkillPenalty = ruleCalculateAttackPenalty.ResultBallisticSkillPenalty;
		ResultBallisticSkill = Math.Max(InitiatorBallisticSkill - BallisticSkillPenalty, 0);
		ResultBaseChancesBeforeRecoil = ((DistanceFactor > 0f) ? Mathf.FloorToInt((float)(ResultBallisticSkill + 30) * DistanceFactor) : 0);
		RuleCalculateStatsWeapon weaponStats = Ability.GetWeaponStats();
		if (weaponStats.ResultAdditionalHitChance != 0)
		{
			HitChanceValueModifiers.Add(weaponStats.ResultAdditionalHitChance, this, ModifierDescriptor.Weapon);
		}
		WeaponStatsResultRecoil = weaponStats.ResultRecoil;
		int num = Mathf.RoundToInt((float)((IsScatter ? Math.Max(0, ResultBaseChancesBeforeRecoil - WeaponStatsResultRecoil) : Math.Max(0, ResultBaseChancesBeforeRecoil)) + HitChanceValueModifiers.Value) * OverpenetrationModifier);
		int hitChanceOverkillBorder = BlueprintRoot.Instance.WarhammerRoot.CombatRoot.HitChanceOverkillBorder;
		RighteousFuryChanceRule.ChanceModifiers.Add(Mathf.Max(0, num - hitChanceOverkillBorder), this, ModifierDescriptor.HitChanceOverkill);
		RawResult = num;
		ResultHitChanceNoUpperLimit = Math.Max(0, RawResult);
		ResultHitChance = Mathf.Clamp(RawResult, 0, hitChanceOverkillBorder);
		if (!Ability.IsBurstAttack && Target is DestructibleEntity)
		{
			RawResult = 100;
			ResultHitChance = 100;
		}
	}

	public void SetFakeCover(LosCalculations.CoverType fakeCover, bool ignoreRealCover)
	{
		FakeCover = fakeCover;
		IgnoreRealCoverByFakeCover = ignoreRealCover;
	}
}
