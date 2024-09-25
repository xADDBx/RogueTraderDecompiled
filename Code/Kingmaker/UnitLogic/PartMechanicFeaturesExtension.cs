using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Enums;

namespace Kingmaker.UnitLogic;

public static class PartMechanicFeaturesExtension
{
	public static bool HasMechanicFeature(this MechanicEntity entity, MechanicsFeatureType type)
	{
		return entity.GetMechanicFeature(type);
	}

	public static FeatureCountableFlag GetMechanicFeature(this MechanicEntity entity, MechanicsFeatureType type)
	{
		PartMechanicFeatures features = entity.Features;
		return type switch
		{
			MechanicsFeatureType.Invalid => throw new Exception("Invalid type"), 
			MechanicsFeatureType.CantAct => features.CantAct, 
			MechanicsFeatureType.CantMove => features.CantMove, 
			MechanicsFeatureType.Undying => features.Undying, 
			MechanicsFeatureType.Flying => features.Flying, 
			MechanicsFeatureType.IsUntargetable => features.IsUntargetable, 
			MechanicsFeatureType.Immortality => features.Immortality, 
			MechanicsFeatureType.AllowDyingCondition => features.AllowDyingCondition, 
			MechanicsFeatureType.IsIgnoredByCombat => features.IsIgnoredByCombat, 
			MechanicsFeatureType.Hidden => features.Hidden, 
			MechanicsFeatureType.DoNotProvokeAttacksOfOpportunity => features.DoNotProvokeAttacksOfOpportunity, 
			MechanicsFeatureType.DoNotResetMovementPointsOnAttacks => features.DoNotResetMovementPointsOnAttacks, 
			MechanicsFeatureType.DisableAttacksOfOpportunity => features.DisableAttacksOfOpportunity, 
			MechanicsFeatureType.IgnoreThreateningAreaForMovementCostCalculation => features.IgnoreThreateningAreaForMovementCostCalculation, 
			MechanicsFeatureType.DisablePush => features.DisablePush, 
			MechanicsFeatureType.CanUseBallisticSkillToParry => features.CanUseBallisticSkillToParry, 
			MechanicsFeatureType.DoNotReviveOutOfCombat => features.DoNotReviveOutOfCombat, 
			MechanicsFeatureType.AutoDodge => features.AutoDodge, 
			MechanicsFeatureType.AutoParry => features.AutoParry, 
			MechanicsFeatureType.AutoHit => features.AutoHit, 
			MechanicsFeatureType.AutoMiss => features.AutoMiss, 
			MechanicsFeatureType.ProvidesFullCover => features.ProvidesFullCover, 
			MechanicsFeatureType.IgnoreMediumArmourDodgePenalty => features.IgnoreMediumArmourDodgePenalty, 
			MechanicsFeatureType.CanRerollSavingThrow => features.CanRerollSavingThrow, 
			MechanicsFeatureType.CanShootInMelee => features.CanShootInMelee, 
			MechanicsFeatureType.Vanguard => features.Vanguard, 
			MechanicsFeatureType.CanPassThroughUnits => features.CanPassThroughUnits, 
			MechanicsFeatureType.CannotBeCriticallyHit => features.CannotBeCriticallyHit, 
			MechanicsFeatureType.HealCanCrit => features.HealCanCrit, 
			MechanicsFeatureType.IgnoreWeaponForceMove => features.IgnoreWeaponForceMove, 
			MechanicsFeatureType.IgnoreAnyForceMove => features.IgnoreAnyForceMove, 
			MechanicsFeatureType.CantJumpAside => features.CantJumpAside, 
			MechanicsFeatureType.FreshInjuryImmunity => features.FreshInjuryImmunity, 
			MechanicsFeatureType.ImmuneToMovementPointReduction => features.ImmuneToMovementPointReduction, 
			MechanicsFeatureType.SecondaryCriticalChance => features.SecondaryCriticalChance, 
			MechanicsFeatureType.OverpenetrationDoesNotDecreaseDamage => features.OverpenetrationDoesNotDecreaseDamage, 
			MechanicsFeatureType.DodgeAnything => features.DodgeAnything, 
			MechanicsFeatureType.IsFirstInFight => features.IsFirstInFight, 
			MechanicsFeatureType.IsLastInFight => features.IsLastInFight, 
			MechanicsFeatureType.IgnorePowerArmourDodgePenalty => features.IgnorePowerArmourDodgePenalty, 
			MechanicsFeatureType.AoEDodgeWithoutMovement => features.AoEDodgeWithoutMovement, 
			MechanicsFeatureType.IgnoreMeleeOutnumbering => features.IgnoreMeleeOutnumbering, 
			MechanicsFeatureType.DoesNotCountTurns => features.DoesNotCountTurns, 
			MechanicsFeatureType.HasNoAPPenaltyCostForTwoWeaponFighting => features.HasNoAPPenaltyCostForTwoWeaponFighting, 
			MechanicsFeatureType.IgnoreCoverEfficiency => features.IgnoreCoverEfficiency, 
			MechanicsFeatureType.WitheringShard => features.WitheringShard, 
			MechanicsFeatureType.HiveOutnumber => features.HiveOutnumber, 
			MechanicsFeatureType.PsychicPowersDoNotProvokeAoO => features.PsychicPowersDoNotProvokeAoO, 
			MechanicsFeatureType.RemoveFromInitiative => features.RemoveFromInitiative, 
			MechanicsFeatureType.BlockOverpenetration => features.BlockOverpenetration, 
			MechanicsFeatureType.OnElevator => features.OnElevator, 
			MechanicsFeatureType.RotationForbidden => features.RotationForbidden, 
			MechanicsFeatureType.SuppressedDismember => features.SuppressedDismember, 
			MechanicsFeatureType.SuppressedDecomposition => features.SuppressedDecomposition, 
			MechanicsFeatureType.ShapeFlames => features.ShapeFlames, 
			MechanicsFeatureType.AutoDodgeFriendlyFire => features.AutoDodgeFriendlyFire, 
			MechanicsFeatureType.HealInsteadOfDamageForDOTs => features.HealInsteadOfDamageForDOTs, 
			MechanicsFeatureType.RangedParry => features.RangedParry, 
			MechanicsFeatureType.CanUseBothDesperateMeasureAndHeroicAct => features.CanUseBothDesperateMeasureAndHeroicAct, 
			MechanicsFeatureType.HalfSuperiorityCriticalChance => features.HalfSuperiorityCriticalChance, 
			MechanicsFeatureType.HideRealHealthInUI => features.HideRealHealthInUI, 
			MechanicsFeatureType.DisableSnapToGrid => features.DisableSnapToGrid, 
			MechanicsFeatureType.AllAttacksCountAsAoe => features.AllAttacksCountAsAoe, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}
}
