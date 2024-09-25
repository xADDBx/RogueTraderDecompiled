using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.Exploration;
using UnityEngine;

namespace Kingmaker.Globalmap.Interaction;

[TypeId("8cc11777ee7a46c9a395221dc8753cbb")]
public abstract class BasePointOfInterestComponent : BlueprintComponent
{
	[SerializeField]
	public BlueprintPointOfInterestReference m_PointBlueprint;

	public virtual BlueprintPointOfInterest PointBlueprint => m_PointBlueprint?.Get();
}
