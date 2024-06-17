using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class InteractionBarkSettings : InteractionSettings
{
	public ConditionsReference Condition;

	[ValidateNotNull]
	[StringCreateTemplate(StringCreateTemplateAttribute.StringType.MapObject)]
	public SharedStringAsset Bark;

	[Tooltip("Show bark on MapObject user. By default bark is shown on MapObject.")]
	public bool ShowOnUser;
}
