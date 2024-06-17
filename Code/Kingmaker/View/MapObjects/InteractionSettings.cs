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

namespace Kingmaker.View.MapObjects;

[Serializable]
public class InteractionSettings
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

	public UIInteractionType UIType;

	public bool NotInCombat;

	public bool ShowOvertip;

	public bool AlwaysDisabled;

	[ShowIf("ShowOvertip")]
	public bool ShowHighlight;

	public int ProximityRadius = 2;

	public InteractionType Type = InteractionType.Approach;

	public UnitAnimationInteractionType UseAnimationState;

	[SerializeField]
	[FormerlySerializedAs("Dialog")]
	private BlueprintDialogReference m_Dialog;

	[AkEventReference]
	public string InteractionSound;

	[AkEventReference]
	public string InteractionDisabledSound;

	[AkEventReference]
	public string InteractionStopSound;

	[CanBeNull]
	public TrapObjectView Trap;

	public bool DoNotNeedCollider;

	public bool UnlimitedInteractionsPerRound;

	public bool OverrideActionPointsCost;

	[ShowIf("OverrideActionPointsCost")]
	public int ActionPointsCost;

	public InteractWithToolFXData InteractWithMeltaChargeFXData;

	[Header("Vertical overtip UI correction (pixels)")]
	public float OvertipVerticalCorrection = 60f;

	public BlueprintDialog Dialog => m_Dialog?.Get();

	public bool InteractOnlyWithToolAfterFail { get; set; }
}
