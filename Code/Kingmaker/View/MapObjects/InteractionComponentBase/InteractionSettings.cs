using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects.Traps;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Sound;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.View.MapObjects.InteractionComponentBase;

[Serializable]
public class InteractionSettings : IHideUnusedSettings
{
	[Serializable]
	public class InteractWithToolFXData
	{
		public bool DoNotShowFx;

		[HideIf("DoNotShowFx")]
		public bool OverrideDefaultFx;

		[ShowIf("CanOverrideDefaultFx")]
		public GameObject OverrideFxPrefab;

		[HideIf("DoNotShowFx")]
		public GameObject FxLocator;

		private bool CanOverrideDefaultFx
		{
			get
			{
				if (OverrideDefaultFx)
				{
					return !DoNotShowFx;
				}
				return false;
			}
		}
	}

	[ShowIf("ShouldShowUIType")]
	public UIInteractionType UIType;

	[ShowIf("ShouldShowNotInCombat")]
	public bool NotInCombat;

	[ShowIf("ShouldShowAlwaysDisabled")]
	public bool AlwaysDisabled;

	[ShowIf("ShouldShowShowOvertip")]
	public bool ShowOvertip;

	[ShowIf("ShouldShowShowHighlight")]
	public bool ShowHighlight;

	[ShowIf("ShouldShowProximityRadius")]
	public int ProximityRadius = 2;

	[ShowIf("ShouldShowType")]
	public InteractionType Type = InteractionType.Approach;

	[ShowIf("ShouldShowUseAnimationState")]
	public UnitAnimationInteractionType UseAnimationState;

	[ShowIf("ShouldShowDialog")]
	[SerializeField]
	[FormerlySerializedAs("Dialog")]
	private BlueprintDialogReference m_Dialog;

	[ShowIf("ShouldShowInteractionSound")]
	[AkEventReference]
	public string InteractionSound;

	[ShowIf("ShouldShowInteractionDisabledSound")]
	[AkEventReference]
	public string InteractionDisabledSound;

	[ShowIf("ShouldShowInteractionStopSound")]
	[AkEventReference]
	public string InteractionStopSound;

	[ShowIf("ShouldShowTrap")]
	[CanBeNull]
	public TrapObjectView Trap;

	[ShowIf("ShouldShowDoNotNeedCollider")]
	public bool DoNotNeedCollider;

	[ShowIf("ShouldShowUnlimitedInteractionsPerRound")]
	public bool UnlimitedInteractionsPerRound;

	[ShowIf("ShouldShowOverrideActionPointsCost")]
	public bool OverrideActionPointsCost;

	[ShowIf("OverrideActionPointsCost")]
	public int ActionPointsCost;

	[ShowIf("ShouldShowInteractWithMeltaChargeFXData")]
	public InteractWithToolFXData InteractWithMeltaChargeFXData;

	[ShowIf("ShouldShowOvertipVerticalCorrection")]
	public float OvertipVerticalCorrection;

	private bool ShouldShowShowHighlight
	{
		get
		{
			if (ShouldShowShowOvertip)
			{
				return ShowOvertip;
			}
			return false;
		}
	}

	public virtual bool ShouldShowUIType => true;

	public virtual bool ShouldShowNotInCombat => true;

	public virtual bool ShouldShowAlwaysDisabled => true;

	public virtual bool ShouldShowShowOvertip => true;

	public virtual bool ShouldShowProximityRadius => true;

	public virtual bool ShouldShowType => true;

	public virtual bool ShouldShowUseAnimationState => false;

	public virtual bool ShouldShowDialog => true;

	public virtual bool ShouldShowInteractionSound => true;

	public virtual bool ShouldShowInteractionDisabledSound => true;

	public virtual bool ShouldShowInteractionStopSound => true;

	public virtual bool ShouldShowTrap => true;

	public virtual bool ShouldShowDoNotNeedCollider => true;

	public virtual bool ShouldShowUnlimitedInteractionsPerRound => true;

	public virtual bool ShouldShowOverrideActionPointsCost => true;

	public virtual bool ShouldShowInteractWithMeltaChargeFXData => true;

	public virtual bool ShouldShowOvertipVerticalCorrection => true;

	public BlueprintDialog Dialog => m_Dialog?.Get();

	public bool InteractOnlyWithToolAfterFail { get; set; }
}
