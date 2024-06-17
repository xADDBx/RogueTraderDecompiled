using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.Colonization;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.Exploration;

[TypeId("cf8147101b6b49b2b3bfd5466175acbf")]
public class BlueprintPointOfInterestResources : BlueprintPointOfInterest
{
	[SerializeField]
	private ResourceData m_ExplorationResource;

	public ResourceData ExplorationResource => m_ExplorationResource;
}
