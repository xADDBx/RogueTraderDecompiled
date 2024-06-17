using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
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

	[CanBeNull]
	public RulePerformDodge ResultDodgeRule { get; private set; }

	[CanBeNull]
	public RuleRollParry ResultParryRule { get; private set; }

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

	public int RighteousFuryAmount { get; private set; }

	public bool ShouldHaveBeenRighteousFury { get; private set; }

	public bool IsMelee => Ability.Weapon?.Blueprint.IsMelee ?? false;

	public RulePerformAttackRoll([NotNull] MechanicEntity initiator, [NotNull] MechanicEntity target, [NotNull] AbilityData ability, int burstIndex, bool disableDodgeForAlly, Vector3? effectiveCasterPosition, Vector3? abilityTargetPosition, float overpenetrationModifier = 1f)
		: base(initiator, target)
	{
		Ability = ability;
		BurstIndex = burstIndex;
		m_DisableDodgeForAlly = disableDodgeForAlly;
		HitChanceRule = new RuleCalculateHitChances(initiator, target, ability, burstIndex, effectiveCasterPosition ?? initiator.Position, abilityTargetPosition ?? target.Position, overpenetrationModifier);
	}

	public RulePerformAttackRoll([NotNull] IMechanicEntity initiator, [NotNull] IMechanicEntity target, [NotNull] AbilityData ability, int burstIndex, bool disableDodgeForAlly, Vector3? effectiveCasterPosition, Vector3? abilityTargetPosition)
		: this((MechanicEntity)initiator, (MechanicEntity)target, ability, burstIndex, disableDodgeForAlly, effectiveCasterPosition, abilityTargetPosition)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Rulebook.Trigger(HitChanceRule);
		AttackHitPolicyType attackHitPolicyType = (IsControlledScatterAutoMiss ? AttackHitPolicyType.AutoMiss : ((!Game.Instance.TurnController.TbActive && !(Target is BaseUnitEntity)) ? AttackHitPolicyType.AutoHit : AttackHitPolicyContextData.Current));
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
			TryParry(unitEntity);
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
			RighteousFuryAmount += (((int)ResultSecondRighteousFuryD100 <= HitChanceRule.ResultRighteousFuryChance - 50) ? 1 : 0);
		}
		RighteousFuryAmount += base.InitiatorUnit.Facts.GetComponents<IncreaseCriticalHitMultiplier>().Count();
		Result = (((!ResultIsRighteousFury && !HitChanceRule.AutoCrits.Value) || ShouldHaveBeenRighteousFury) ? AttackResult.Hit : AttackResult.RighteousFury);
	}

	private void TryParry(UnitEntity targetUnit)
	{
		if (targetUnit.CanAct && IsMelee && (targetUnit.GetThreatHandMelee() != null || (targetUnit.Features.CanUseBallisticSkillToParry.Value && targetUnit.GetThreatHandRanged() != null)))
		{
			RuleRollParry evt = new RuleRollParry(targetUnit, base.ConcreteInitiator, Ability, HitChanceRule.ResultSuperiorityNumber);
			ResultParryRule = Rulebook.Trigger(evt);
		}
		RuleRollParry resultParryRule = ResultParryRule;
		if (resultParryRule != null && resultParryRule.Result)
		{
			Result = AttackResult.Parried;
			ResultIsHit = false;
			if (targetUnit.View != null && targetUnit.View.AnimationManager != null)
			{
				targetUnit.View.AnimationManager.Execute(targetUnit.View.AnimationManager.CreateHandle(UnitAnimationType.Parry));
				targetUnit.SetOrientation(targetUnit.GetLookAtAngle(base.ConcreteInitiator.Position));
			}
		}
	}

	private void TryDodge(UnitEntity targetUnit)
	{
		RulePerformDodge dodge = new RulePerformDodge(targetUnit, base.ConcreteInitiator, Ability, HitChanceRule.ResultLos, DangerArea, BurstIndex);
		if (!targetUnit.Features.AutoDodge && (!targetUnit.State.CanDodge || (m_DisableDodgeForAlly && base.ConcreteTarget.IsAlly(base.Initiator)) || (UnitPartJumpAsideDodge.NeedStepAsideDodge(targetUnit, dodge) && !targetUnit.CanMove)))
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
