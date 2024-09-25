using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.Exploration;

namespace Kingmaker.Globalmap.Exploration;

[AllowedOn(typeof(BlueprintAnomaly))]
[TypeId("25be4538ef4f4576be7f775ab5f04eca")]
public abstract class AnomalyInteraction : BlueprintComponent
{
	public abstract void Interact();
}
