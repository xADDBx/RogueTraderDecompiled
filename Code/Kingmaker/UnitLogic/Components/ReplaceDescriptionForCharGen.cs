using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.UnitLogic.Components;

[AllowedOn(typeof(BlueprintFeature))]
[TypeId("ce652167229b45e09a9329804b905a2b")]
public class ReplaceDescriptionForCharGen : BlueprintComponent
{
	public LocalizedString CharGenDescription;
}
