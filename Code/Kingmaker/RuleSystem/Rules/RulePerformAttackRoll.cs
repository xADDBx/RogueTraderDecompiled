using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Enums;
using Kingmaker.Items.Slots;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Animation.Kingmaker;
using UnityEngine;

namespace Kingmaker.RuleSystem.Rules;

public class RulePerformAttackRoll : RulebookTargetEvent
{
	private readonly bool m_DisableDodgeForAlly;

	public readonly ValueModifiersManager HitChanceValueModifiers = new ValueModifiersManager();

	public readonly HashSet<CustomGridNodeBase> DangerArea = new HashSet<CustomGridNodeBase>();

	public AbilityData Ability { get; }

	public int BurstIndex { get; }

	public RuleCalculateHitChances HitChanceRule { get; }

	public bool IsControlledScatterAutoMiss { get; set; }

	public bool IsOverpenetration { get; set; }

	public bool OverrideAttackHitPolicy { get; set; }

	public AttackHitPolicyType AttackHitPolicyType { get; set; }

	[CanBeNull]
	public RulePerformDodge ResultDodgeRule { get; private set; }

	[CanBeNull]
	public RuleRollParry ResultParryRule { get; private set; }

	public MechanicEntity ActualParryUnit { get; private set; }

	public RuleRollChance ResultChanceRule { get; private set; }

	[CanBeNull]
	public RuleRollCoverHit ResultRollCoverHitRule { get; private set; }

	[CanBeNull]
	public RuleRollD100 ResultRighteousFuryD100 { get; private set; }

	[CanBeNull]
	public RuleRollD100 ResultSecondRighteousFuryD100 { get; private set; }

	public HitLocation ResultHitLocation { get; private set; }

	public AttackResult Result { get; private set; }

	public bool ResultIsHit { get; private set; }

	public bool ResultIsCoverHit { get; private set; }

	public bool ResultDamageIsHalvedBecauseOfAoEMiss { get; private set; }

	[CanBeNull]
	public MechanicEntity ResultCoverEntity { get; private set; }

	public bool ResultIsRighteousFury { get; private set; }

	public float RighteousFuryAmount { get; private set; }

	public bool ShouldHaveBeenRighteousFury { get; private set; }

	public bool IsMelee => Ability.Weapon?.Blueprint.IsMelee ?? false;

	public RulePerformAttackRoll([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [NotNull] AbilityData ability, int burstIndex, bool disableDodgeForAlly, Vector3? effectiveCasterPosition, Vector3? abilityTargetPosition, float overpenetrationModifier = 1f)
		: base(initiator, target)
	{
		Ability = ability;
		BurstIndex = burstIndex;
		m_DisableDodgeForAlly = disableDodgeForAlly;
		ActualParryUnit = target;
		HitChanceRule = new RuleCalculateHitChances(initiator, target, ability, burstIndex, effectiveCasterPosition ?? initiator.Position, abilityTargetPosition ?? target.Position, overpenetrationModifier);
	}

	public RulePerformAttackRoll([NotNull] IMechanicEntity initiator, [NotNull] IMechanicEntity target, [NotNull] AbilityData ability, int burstIndex, bool disableDodgeForAlly, Vector3? effectiveCasterPosition, Vector3? abilityTargetPosition)
		: this((MechanicEntity)initiator, (MechanicEntity)target, ability, burstIndex, disableDodgeForAlly, effectiveCasterPosition, abilityTargetPosition)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Rulebook.Trigger(HitChanceRule);
		AttackHitPolicyType attackHitPolicyType = (OverrideAttackHitPolicy ? AttackHitPolicyType : (IsControlledScatterAutoMiss ? AttackHitPolicyType.AutoMiss : ((!Game.Instance.TurnController.TbActive && !(Target is BaseUnitEntity)) ? AttackHitPolicyType.AutoHit : AttackHitPolicyContextData.Current)));
		bool flag = attackHitPolicyType == AttackHitPolicyType.Default;
		HitChanceValueModifiers.CopyFrom(HitChanceRule.HitChanceValueModifiers);
		ResultChanceRule = RuleRollChance.Roll(Rulebook.CurrentContext.Current?.Initiator, HitChanceRule.ResultHitChance, RollType.Attack, RollChanceType.Untyped, Rulebook.CurrentContext.Current?.Initiator);
		bool flag2 = attackHitPolicyType switch
		{
			AttackHitPolicyType.Default => ResultChanceRule.Success, 
			AttackHitPolicyType.AutoHit => true, 
			AttackHitPolicyType.AutoMiss => false, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (!flag2 && Ability.IsAOE && flag)
		{
			flag2 = (ResultDamageIsHalvedBecauseOfAoEMiss = true);
		}
		ResultIsHit = flag2;
		if (HitChanceRule.ResultCoverHitChanceRule != null)
		{
			RuleRollCoverHit ruleRollCoverHit2 = (ResultRollCoverHitRule = Rulebook.Trigger(new RuleRollCoverHit(HitChanceRule.ResultCoverHitChanceRule, HitChanceRule.IsAutoHit)));
			RuleRollCoverHit ruleRollCoverHit3 = ruleRollCoverHit2;
			ResultIsCoverHit = attackHitPolicyType switch
			{
				AttackHitPolicyType.Default => ruleRollCoverHit3.ResultIsHit, 
				AttackHitPolicyType.AutoHit => false, 
				AttackHitPolicyType.AutoMiss => false, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		if (ResultIsCoverHit)
		{
			Result = AttackResult.CoverHit;
			ResultCoverEntity = HitChanceRule.ResultCoverEntity;
			return;
		}
		if (!ResultIsHit)
		{
			Result = AttackResult.Miss;
			return;
		}
		if (flag && Target is UnitEntity unitEntity)
		{
			TryDodge(unitEntity);
			TryParry(unitEntity, context);
			if (!ResultIsHit)
			{
				return;
			}
			if (unitEntity.HasMechanicFeature(MechanicsFeatureType.CannotBeCriticallyHit))
			{
				ShouldHaveBeenRighteousFury = true;
			}
		}
		if (ResultHitLocation == HitLocation.Undefined)
		{
			int num = (int)ResultChanceRule / 10;
			num = (int)ResultChanceRule - num * 10;
			num = num * 10 + (int)ResultChanceRule / 10;
			ResultHitLocation = ((num <= 10) ? HitLocation.Head : ((num <= 30) ? HitLocation.Arms : ((num <= 70) ? HitLocation.Body : HitLocation.Legs)));
		}
		if (HitChanceRule.ResultRighteousFuryChance > 0 || HitChanceRule.AutoCrits.Value)
		{
			ResultRighteousFuryD100 = Dice.D100;
			ResultIsRighteousFury = ((int)ResultRighteousFuryD100 <= HitChanceRule.ResultRighteousFuryChance || HitChanceRule.AutoCrits.Value) && !ShouldHaveBeenRighteousFury;
			RighteousFuryAmount += (ResultIsRighteousFury ? 1 : 0);
		}
		if ((bool)(base.Initiator as UnitEntity)?.Features.SecondaryCriticalChance)
		{
			ResultSecondRighteousFuryD100 = Dice.D100;
			RighteousFuryAmount += (((int)ResultSecondRighteousFuryD100 <= HitChanceRule.ResultRighteousFuryChance - 50) ? 0.5f : 0f);
		}
		RighteousFuryAmount += base.InitiatorUnit.Facts.GetComponents<IncreaseCriticalHitMultiplier>().Count();
		Result = (((!ResultIsRighteousFury && !HitChanceRule.AutoCrits.Value) || ShouldHaveBeenRighteousFury) ? AttackResult.Hit : AttackResult.RighteousFury);
	}

	private void TryParry(UnitEntity targetUnit, RulebookEventContext context)
	{
		if (IsMelee && CanParryMelee(targetUnit))
		{
			RuleRollParry evt = new RuleRollParry(targetUnit, base.ConcreteInitiator, Ability, HitChanceRule.ResultSuperiorityNumber);
			ResultParryRule = Rulebook.Trigger(evt);
		}
		if (!IsMelee && CanParryRanged(targetUnit))
		{
			int hitChance;
			if (Ability.IsScatter)
			{
				RuleCalculateScatterShotHitDirectionProbability ruleCalculateScatterShotHitDirectionProbability = Rulebook.Trigger(new RuleCalculateScatterShotHitDirectionProbability(base.ConcreteInitiator, Ability, 1));
				hitChance = (base.ConcreteInitiator.InRangeInCells(targetUnit, Ability.Weapon?.Blueprint.AttackOptimalRange ?? 1) ? ruleCalculateScatterShotHitDirectionProbability.ResultMainLineNear : ruleCalculateScatterShotHitDirectionProbability.ResultMainLine);
			}
			else
			{
				if (Ability.IsAOE)
				{
					return;
				}
				hitChance = HitChanceRule.ResultHitChance;
			}
			RuleRollParry evt2 = new RuleRollParry(targetUnit, base.ConcreteInitiator, Ability, HitChanceRule.ResultSuperiorityNumber, rangedParry: true, hitChance);
			ResultParryRule = Rulebook.Trigger(evt2);
		}
		RuleRollParry resultParryRule = ResultParryRule;
		if (resultParryRule != null && resultParryRule.Result)
		{
			Result = AttackResult.Parried;
			ResultIsHit = false;
			if (!IsMelee || targetUnit.HasMechanicFeature(MechanicsFeatureType.RangedParry) || !(targetUnit.View != null) || !(targetUnit.View.AnimationManager != null))
			{
				return;
			}
			UnitAnimationActionHandle unitAnimationActionHandle = targetUnit.View.AnimationManager.CreateHandle(UnitAnimationType.Parry);
			if (targetUnit.GetThreatHandMelee() != null || (targetUnit.Features.CanUseBallisticSkillToParry.Value && targetUnit.GetThreatHandRanged() != null))
			{
				WeaponSlot weaponSlot = ((targetUnit.GetThreatHandMelee() != null) ? targetUnit.GetThreatHandMelee() : ((targetUnit.Features.CanUseBallisticSkillToParry.Value && targetUnit.GetThreatHandRanged() != null) ? targetUnit.GetThreatHandRanged() : null));
				if (weaponSlot != null && weaponSlot is HandSlot handSlot)
				{
					unitAnimationActionHandle.CastInOffhand = !handSlot.IsPrimaryHand;
				}
			}
			targetUnit.View.AnimationManager.Execute(unitAnimationActionHandle);
			targetUnit.SetOrientation(targetUnit.GetLookAtAngle(base.ConcreteInitiator.Position));
			return;
		}
		List<EntityRef<BaseUnitEntity>> list = CombatEngagementHelper.CollectUnitsAround(targetUnit, (BaseUnitEntity entity) => entity.IsAlly(targetUnit) && entity.Facts.HasComponent<WarhammerAllyParry>());
		if (list.Empty())
		{
			return;
		}
		foreach (EntityRef<BaseUnitEntity> item in list)
		{
			BaseUnitEntity baseUnitEntity = item;
			WarhammerAllyParry warhammerAllyParry = baseUnitEntity.Facts.GetComponents<WarhammerAllyParry>().First();
			if (IsMelee && warhammerAllyParry.ParryMelee && CanParryMelee(baseUnitEntity))
			{
				RuleRollParry ruleRollParry = Rulebook.Trigger(new RuleRollParry(baseUnitEntity as UnitEntity, base.ConcreteInitiator, Ability, HitChanceRule.ResultSuperiorityNumber));
				if (ruleRollParry != null && ruleRollParry.Result)
				{
					ResultParryRule = ruleRollParry;
					Result = AttackResult.Parried;
					ResultIsHit = false;
					ActualParryUnit = baseUnitEntity;
					break;
				}
			}
			if (IsMelee || !warhammerAllyParry.ParryRanged || !CanParryRanged(baseUnitEntity))
			{
				continue;
			}
			int hitChance2;
			if (Ability.IsScatter)
			{
				RuleCalculateScatterShotHitDirectionProbability ruleCalculateScatterShotHitDirectionProbability2 = Rulebook.Trigger(new RuleCalculateScatterShotHitDirectionProbability(base.ConcreteInitiator, Ability, 1));
				hitChance2 = (base.ConcreteInitiator.InRangeInCells(baseUnitEntity, Ability.Weapon?.Blueprint.AttackOptimalRange ?? 1) ? ruleCalculateScatterShotHitDirectionProbability2.ResultMainLineNear : ruleCalculateScatterShotHitDirectionProbability2.ResultMainLine);
			}
			else
			{
				if (Ability.IsAOE)
				{
					continue;
				}
				hitChance2 = HitChanceRule.ResultHitChance;
			}
			RuleRollParry ruleRollParry2 = Rulebook.Trigger(new RuleRollParry(baseUnitEntity as UnitEntity, base.ConcreteInitiator, Ability, HitChanceRule.ResultSuperiorityNumber, rangedParry: true, hitChance2));
			if (ruleRollParry2 != null && ruleRollParry2.Result)
			{
				ResultParryRule = ruleRollParry2;
				Result = AttackResult.Parried;
				ResultIsHit = false;
				ActualParryUnit = baseUnitEntity;
				break;
			}
		}
	}

	private bool CanParryMelee(BaseUnitEntity unit)
	{
		if (unit.CanAct)
		{
			if (unit.GetThreatHandMelee() == null)
			{
				if (unit.Features.CanUseBallisticSkillToParry.Value)
				{
					return unit.GetThreatHandRanged() != null;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private bool CanParryRanged(BaseUnitEntity unit)
	{
		if (unit.CanAct && unit.HasMechanicFeature(MechanicsFeatureType.RangedParry))
		{
			return base.ConcreteInitiator.Facts.GetComponents<WarhammerDeflectionTarget>().Any((WarhammerDeflectionTarget c) => c.Caster == unit);
		}
		return false;
	}

	private void TryDodge(UnitEntity targetUnit)
	{
		RulePerformDodge dodge = new RulePerformDodge(targetUnit, base.ConcreteInitiator, Ability, HitChanceRule.ResultLos, DangerArea, BurstIndex);
		if (!targetUnit.Features.AutoDodge && (!targetUnit.Features.AutoDodgeFriendlyFire || !targetUnit.IsAlly(base.Initiator)) && (!targetUnit.State.CanDodge || (m_DisableDodgeForAlly && base.ConcreteTarget.IsAlly(base.Initiator)) || (UnitPartJumpAsideDodge.NeedStepAsideDodge(targetUnit, dodge) && !targetUnit.CanMove)))
		{
			return;
		}
		if (HitChanceRule.IsScatter)
		{
			dodge.ChancesRule.DodgeValueModifiers.Add(-10, this, ModifierDescriptor.ScatterShot);
		}
		ResultDodgeRule = Rulebook.Trigger(dodge);
		RulePerformDodge resultDodgeRule = ResultDodgeRule;
		if (resultDodgeRule == null || !resultDodgeRule.Result)
		{
			return;
		}
		Result = AttackResult.Dodge;
		if (dodge.IsJumpAside)
		{
			EventBus.RaiseEvent(delegate(IDodgeHandler h)
			{
				h.HandleJumpAsideDodge(dodge);
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(IDodgeHandler h)
			{
				h.HandleSimpleDodge(dodge);
			});
		}
		ResultIsHit = false;
	}
}
