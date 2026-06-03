using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintUnit))]
[TypeId("c506b513ed3ea6549a459be4facb8732")]
public class UniqueEogannCompanionComponent : BlueprintComponent
{
	public bool AlwaysShowWeaponsInMechadendrites = true;
}
