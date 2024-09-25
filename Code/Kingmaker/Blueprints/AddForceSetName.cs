using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintFeature))]
[TypeId("22763793998e4e87a8051557fb3b0040")]
public class AddForceSetName : BlueprintComponent
{
	public LocalizedString ForceName;
}
