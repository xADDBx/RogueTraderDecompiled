using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public class PartMechanicFeatures : MechanicEntityPart, IHashable
{
	public FeatureCountableFlag Undying;

	public FeatureCountableFlag DoNotProvokeAttacksOfOpportunity;

	public FeatureCountableFlag Flying;

	public FeatureCountableFlag IsUntargetable;

	public FeatureCountableFlag Immortality;

	public FeatureCountableFlag AllowDyingCondition;

	public FeatureCountableFlag IsIgnoredByCombat;

	public FeatureCountableFlag Hidden;

	public FeatureCountableFlag OnElevator;

	public FeatureCountableFlag DoNotResetMovementPointsOnAttacks;

	public FeatureCountableFlag RotationForbidden;

	public FeatureCountableFlag IgnoreThreateningAreaForMovementCostCalculation;

	public FeatureCountableFlag DisablePush;

	public FeatureCountableFlag CanUseBallisticSkillToParry;

	public FeatureCountableFlag DoNotReviveOutOfCombat;

	public FeatureCountableFlag AutoDodge;

	public FeatureCountableFlag AutoParry;

	public FeatureCountableFlag AutoHit;

	public FeatureCountableFlag AutoMiss;

	public FeatureCountableFlag ProvidesFullCover;

	public FeatureCountableFlag CanRerollSavingThrow;

	public FeatureCountableFlag IgnoreMediumArmourDodgePenalty;

	public FeatureCountableFlag CanShootInMelee;

	public FeatureCountableFlag CanPassThroughUnits;

	public FeatureCountableFlag CannotBeCriticallyHit;

	public FeatureCountableFlag HealCanCrit;

	public FeatureCountableFlag IgnoreWeaponForceMove;

	public FeatureCountableFlag IgnoreAnyForceMove;

	public FeatureCountableFlag CantJumpAside;

	public FeatureCountableFlag FreshInjuryImmunity;

	public FeatureCountableFlag ImmuneToMovementPointReduction;

	public FeatureCountableFlag SecondaryCriticalChance;

	public FeatureCountableFlag OverpenetrationDoesNotDecreaseDamage;

	public FeatureCountableFlag DodgeAnything;

	public FeatureCountableFlag IsFirstInFight;

	public FeatureCountableFlag IsLastInFight;

	public FeatureCountableFlag IgnorePowerArmourDodgePenalty;

	public FeatureCountableFlag AoEDodgeWithoutMovement;

	public FeatureCountableFlag IgnoreMeleeOutnumbering;

	public FeatureCountableFlag DoesNotCountTurns;

	public FeatureCountableFlag HasNoAPPenaltyCostForTwoWeaponFighting;

	public FeatureCountableFlag IgnoreCoverEfficiency;

	public FeatureCountableFlag WitheringShard;

	public FeatureCountableFlag HiveOutnumber;

	public FeatureCountableFlag PsychicPowersDoNotProvokeAoO;

	public FeatureCountableFlag BlockOverpenetration;

	public FeatureCountableFlag ShapeFlames;

	public FeatureCountableFlag AutoDodgeFriendlyFire;

	public FeatureCountableFlag HealInsteadOfDamageForDOTs;

	public FeatureCountableFlag RangedParry;

	public FeatureCountableFlag CanUseBothDesperateMeasureAndHeroicAct;

	public FeatureCountableFlag HalfSuperiorityCriticalChance;

	public FeatureCountableFlag HideRealHealthInUI;

	public FeatureCountableFlag DisableSnapToGrid;

	public FeatureCountableFlag CantAct;

	public FeatureCountableFlag CantMove;

	public FeatureCountableFlag DisableAttacksOfOpportunity;

	public FeatureCountableFlag Vanguard;

	public FeatureCountableFlag RemoveFromInitiative;

	public FeatureCountableFlag SuppressedDismember;

	public FeatureCountableFlag SuppressedDecomposition;

	public FeatureCountableFlag AllAttacksCountAsAoe;

	public void Initialize()
	{
		Undying = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.Undying);
		DoNotProvokeAttacksOfOpportunity = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.DoNotProvokeAttacksOfOpportunity);
		Flying = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.Flying);
		IsUntargetable = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.IsUntargetable);
		Immortality = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.Immortality);
		AllowDyingCondition = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.AllowDyingCondition);
		IsIgnoredByCombat = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.IsIgnoredByCombat);
		Hidden = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.Hidden);
		OnElevator = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.OnElevator);
		DoNotResetMovementPointsOnAttacks = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.DoNotResetMovementPointsOnAttacks);
		RotationForbidden = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.RotationForbidden);
		IgnoreThreateningAreaForMovementCostCalculation = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.IgnoreThreateningAreaForMovementCostCalculation);
		DisablePush = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.DisablePush);
		CantAct = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.CantAct);
		CantMove = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.CantMove);
		DisableAttacksOfOpportunity = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.DisableAttacksOfOpportunity);
		SuppressedDismember = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.SuppressedDismember);
		SuppressedDecomposition = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.SuppressedDecomposition);
		CanUseBallisticSkillToParry = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.CanUseBallisticSkillToParry);
		DoNotReviveOutOfCombat = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.DoNotReviveOutOfCombat);
		AutoDodge = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.AutoDodge);
		AutoParry = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.AutoParry);
		AutoHit = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.AutoHit);
		AutoMiss = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.AutoMiss);
		ProvidesFullCover = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.ProvidesFullCover);
		IgnoreMediumArmourDodgePenalty = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.IgnoreMediumArmourDodgePenalty);
		CanRerollSavingThrow = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.CanRerollSavingThrow);
		CanShootInMelee = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.CanShootInMelee);
		Vanguard = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.Vanguard);
		CanPassThroughUnits = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.CanPassThroughUnits);
		CannotBeCriticallyHit = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.CannotBeCriticallyHit);
		HealCanCrit = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.HealCanCrit);
		IgnoreWeaponForceMove = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.IgnoreWeaponForceMove);
		IgnoreAnyForceMove = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.IgnoreAnyForceMove);
		CantJumpAside = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.CantJumpAside);
		FreshInjuryImmunity = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.FreshInjuryImmunity);
		ImmuneToMovementPointReduction = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.ImmuneToMovementPointReduction);
		SecondaryCriticalChance = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.SecondaryCriticalChance);
		OverpenetrationDoesNotDecreaseDamage = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.OverpenetrationDoesNotDecreaseDamage);
		DodgeAnything = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.DodgeAnything);
		IsFirstInFight = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.IsFirstInFight);
		IsLastInFight = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.IsLastInFight);
		IgnorePowerArmourDodgePenalty = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.IgnorePowerArmourDodgePenalty);
		AoEDodgeWithoutMovement = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.AoEDodgeWithoutMovement);
		IgnoreMeleeOutnumbering = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.IgnoreMeleeOutnumbering);
		DoesNotCountTurns = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.DoesNotCountTurns);
		HasNoAPPenaltyCostForTwoWeaponFighting = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.HasNoAPPenaltyCostForTwoWeaponFighting);
		IgnoreCoverEfficiency = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.IgnoreCoverEfficiency);
		WitheringShard = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.WitheringShard);
		HiveOutnumber = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.HiveOutnumber);
		PsychicPowersDoNotProvokeAoO = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.PsychicPowersDoNotProvokeAoO);
		RemoveFromInitiative = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.RemoveFromInitiative);
		BlockOverpenetration = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.BlockOverpenetration);
		ShapeFlames = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.ShapeFlames);
		AutoDodgeFriendlyFire = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.AutoDodgeFriendlyFire);
		HealInsteadOfDamageForDOTs = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.HealInsteadOfDamageForDOTs);
		RangedParry = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.RangedParry);
		CanUseBothDesperateMeasureAndHeroicAct = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.CanUseBothDesperateMeasureAndHeroicAct);
		HalfSuperiorityCriticalChance = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.HalfSuperiorityCriticalChance);
		HideRealHealthInUI = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.HideRealHealthInUI);
		DisableSnapToGrid = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.DisableSnapToGrid);
		AllAttacksCountAsAoe = new FeatureCountableFlag(base.Owner, MechanicsFeatureType.AllAttacksCountAsAoe);
	}

	protected override void OnAttach()
	{
		base.OnAttach();
		Initialize();
	}

	protected override void OnPrePostLoad()
	{
		base.OnPrePostLoad();
		Initialize();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
