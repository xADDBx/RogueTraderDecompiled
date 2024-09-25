using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.Exploration;

[TypeId("99a3644cfd7fbef43b5eea4bf3eac4db")]
public class BlueprintPointOfInterestCargo : BlueprintPointOfInterest
{
	[SerializeField]
	private List<BlueprintCargoReference> m_ExplorationCargo = new List<BlueprintCargoReference>();

	public List<BlueprintCargo> ExplorationCargo => m_ExplorationCargo?.Dereference().ToList();
}
