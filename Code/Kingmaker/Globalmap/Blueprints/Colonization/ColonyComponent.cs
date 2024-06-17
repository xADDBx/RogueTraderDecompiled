using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.SystemMap;

namespace Kingmaker.Globalmap.Blueprints.Colonization;

[AllowedOn(typeof(BlueprintPlanet))]
[TypeId("1ff28703109a4697bd8e35b30aeda286")]
public class ColonyComponent : BlueprintComponent
{
	public BlueprintColonyReference ColonyBlueprint;
}
