using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class InteractionActionSettings : InteractionSettings
{
	[StringCreateTemplate(StringCreateTemplateAttribute.StringType.MapObject)]
	public SharedStringAsset DisplayName;

	public bool DisableAfterUse;

	[Tooltip("Overrides UI Type to make the object appear lootable: shows loot cursor and loot display name when Display Name is empty")]
	public bool IsFakeLoot;

	[ShowCreator]
	public ConditionsReference Condition;

	[ShowCreator]
	public ActionsReference Actions;
}
