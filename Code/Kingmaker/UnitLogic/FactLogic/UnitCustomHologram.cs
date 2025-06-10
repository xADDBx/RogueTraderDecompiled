using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ResourceLinks;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnit))]
[TypeId("01f2d6a22454457b9bba8d628d4d45d3")]
public class UnitCustomHologram : BlueprintComponent
{
	public PrefabLink Prefab;
}
