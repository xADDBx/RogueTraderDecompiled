using System;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class InteractionBarkSettings : InteractionSettings
{
	[ValidateNotNull]
	[StringCreateTemplate(StringCreateTemplateAttribute.StringType.MapObject)]
	public SharedStringAsset Bark;

	[CanBeNull]
	public ActionsReference BarkActions;

	public bool RunActionsOnce;

	[Tooltip("Show bark on MapObject user. By default bark is shown on MapObject.")]
	public bool ShowOnUser;

	public ConditionsReference Condition;

	public override bool ShouldShowUseAnimationState => false;

	public override bool ShouldShowDialog => false;

	public override bool ShouldShowUnlimitedInteractionsPerRound => false;

	public override bool ShouldShowOverrideActionPointsCost => false;

	public override bool ShouldShowInteractWithMeltaChargeFXData => false;

	public bool ActionsRan { get; set; }
}
