using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.Exploration;

namespace Kingmaker.Globalmap.Interaction;

[AllowMultipleComponents]
[TypeId("3ad85baec230fcf4a9b0f4bc9bd6aa80")]
public class PointOfInterestCargoComponent : BasePointOfInterestComponent
{
	public new BlueprintPointOfInterestCargo PointBlueprint => (BlueprintPointOfInterestCargo)base.PointBlueprint;
}
