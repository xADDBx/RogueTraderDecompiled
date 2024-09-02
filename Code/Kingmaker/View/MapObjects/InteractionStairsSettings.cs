using System;
using Kingmaker.Pathfinding;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class InteractionStairsSettings : InteractionSettings
{
	[SerializeField]
	public WarhammerNodeLink NodeLink;

	public override bool ShouldShowUIType => false;

	public override bool ShouldShowNotInCombat => false;

	public override bool ShouldShowAlwaysDisabled => false;

	public override bool ShouldShowShowOvertip => false;

	public override bool ShouldShowProximityRadius => false;

	public override bool ShouldShowType => false;

	public override bool ShouldShowUseAnimationState => false;

	public override bool ShouldShowDialog => false;

	public override bool ShouldShowInteractionSound => false;

	public override bool ShouldShowInteractionDisabledSound => false;

	public override bool ShouldShowInteractionStopSound => false;

	public override bool ShouldShowTrap => false;

	public override bool ShouldShowUnlimitedInteractionsPerRound => false;

	public override bool ShouldShowOverrideActionPointsCost => false;

	public override bool ShouldShowInteractWithMeltaChargeFXData => false;

	public override bool ShouldShowOvertipVerticalCorrection => false;
}
