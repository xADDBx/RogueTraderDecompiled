using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using UnityEngine;

namespace Kingmaker.Globalmap.Interaction;

[AllowedOn(typeof(BlueprintPlanet))]
[TypeId("e7db42e2087b4848be05526506ffc70e")]
public class AlternativePlanetBlueprint : BlueprintComponent
{
	[SerializeField]
	private BlueprintPlanet.Reference m_Planet;

	public BlueprintPlanet Planet => m_Planet.Get();
}
