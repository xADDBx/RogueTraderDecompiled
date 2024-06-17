using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.Exploration;

namespace Kingmaker.Globalmap.Interaction;

[AllowMultipleComponents]
[TypeId("a43ec46b34714baeadccaffda2641bb8")]
public class PointOfInterestBookEventComponent : BasePointOfInterestComponent
{
	public new BlueprintPointOfInterestBookEvent PointBlueprint => (BlueprintPointOfInterestBookEvent)base.PointBlueprint;
}
