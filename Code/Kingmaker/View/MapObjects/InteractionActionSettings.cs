using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.View.MapObjects.InteractionComponentBase;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class InteractionActionSettings : InteractionSettings
{
	[StringCreateTemplate(StringCreateTemplateAttribute.StringType.MapObject)]
	public SharedStringAsset DisplayName;

	public bool DisableAfterUse;

	[ShowCreator]
	public ConditionsReference Condition;

	[ShowCreator]
	public ActionsReference Actions;
}
