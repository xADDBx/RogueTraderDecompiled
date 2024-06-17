using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Localization;

namespace Kingmaker.Globalmap.Blueprints.Colonization;

[TypeId("aa1696d025c449f18968245aa30f56fd")]
public class BlueprintColony : BlueprintScriptableObject
{
	public LocalizedString Name;

	public BlueprintColonyProjectReference[] Projects;

	public int InitialContentment;

	public int InitialSecurity;

	public int InitialEfficiency;

	public ResourceData[] ResourcesProducedFromStart;

	public BlueprintColonyTrait.Reference[] Traits;

	public BlueprintPlanetPrefab.Reference PlanetVisual;

	public string ColonyName => Name;
}
