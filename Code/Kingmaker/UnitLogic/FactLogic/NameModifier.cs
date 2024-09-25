using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintFeature))]
[AllowMultipleComponents]
[TypeId("058e37ed1490441995e349f52d8bb440")]
public abstract class NameModifier : BlueprintComponent
{
	public abstract string Modify(string originalString);
}
