using System.Collections.Generic;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Networking.Serialization;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Globalmap.SystemMap;

public class StarSystemsState : IHashable
{
	[JsonProperty]
	public List<ExpeditionInfo> SentExpedition = new List<ExpeditionInfo>();

	[JsonProperty]
	public List<PlanetExplorationInfo> ScannedPlanets = new List<PlanetExplorationInfo>();

	[JsonProperty]
	public List<BlueprintStarSystemMap> StarSystemsToVisit = new List<BlueprintStarSystemMap>();

	[JsonProperty]
	public List<BlueprintPointOfInterest> PointsExploredOutsideSystemMap = new List<BlueprintPointOfInterest>();

	[JsonProperty]
	public List<BlueprintAnomaly> AnomalySetToNonInteractable = new List<BlueprintAnomaly>();

	[JsonProperty]
	public List<AstropathBriefInfo> AstropathBriefs = new List<AstropathBriefInfo>();

	[JsonProperty]
	[HasherCustom(Type = typeof(DictionaryBlueprintToListOfBlueprintsHasher<BlueprintStarSystemMap, BlueprintPointOfInterest>))]
	public Dictionary<BlueprintStarSystemMap, List<BlueprintPointOfInterest>> InteractedPoints = new Dictionary<BlueprintStarSystemMap, List<BlueprintPointOfInterest>>();

	[JsonProperty]
	[HasherCustom(Type = typeof(DictionaryBlueprintToListOfBlueprintsHasher<BlueprintStarSystemMap, BlueprintAnomaly>))]
	public Dictionary<BlueprintStarSystemMap, List<BlueprintAnomaly>> InteractedAnomalies = new Dictionary<BlueprintStarSystemMap, List<BlueprintAnomaly>>();

	[JsonProperty]
	public Dictionary<BlueprintPlanet, BlueprintPlanetPrefab> PlanetChangedVisualPrefabs = new Dictionary<BlueprintPlanet, BlueprintPlanetPrefab>();

	[JsonProperty]
	[GameStateIgnore]
	public int TutorialSsoCount;

	public StarSystemContextData StarSystemContextData = new StarSystemContextData();

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		List<ExpeditionInfo> sentExpedition = SentExpedition;
		if (sentExpedition != null)
		{
			for (int i = 0; i < sentExpedition.Count; i++)
			{
				Hash128 val = ClassHasher<ExpeditionInfo>.GetHash128(sentExpedition[i]);
				result.Append(ref val);
			}
		}
		List<PlanetExplorationInfo> scannedPlanets = ScannedPlanets;
		if (scannedPlanets != null)
		{
			for (int j = 0; j < scannedPlanets.Count; j++)
			{
				Hash128 val2 = ClassHasher<PlanetExplorationInfo>.GetHash128(scannedPlanets[j]);
				result.Append(ref val2);
			}
		}
		List<BlueprintStarSystemMap> starSystemsToVisit = StarSystemsToVisit;
		if (starSystemsToVisit != null)
		{
			for (int k = 0; k < starSystemsToVisit.Count; k++)
			{
				Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(starSystemsToVisit[k]);
				result.Append(ref val3);
			}
		}
		List<BlueprintPointOfInterest> pointsExploredOutsideSystemMap = PointsExploredOutsideSystemMap;
		if (pointsExploredOutsideSystemMap != null)
		{
			for (int l = 0; l < pointsExploredOutsideSystemMap.Count; l++)
			{
				Hash128 val4 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(pointsExploredOutsideSystemMap[l]);
				result.Append(ref val4);
			}
		}
		List<BlueprintAnomaly> anomalySetToNonInteractable = AnomalySetToNonInteractable;
		if (anomalySetToNonInteractable != null)
		{
			for (int m = 0; m < anomalySetToNonInteractable.Count; m++)
			{
				Hash128 val5 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(anomalySetToNonInteractable[m]);
				result.Append(ref val5);
			}
		}
		List<AstropathBriefInfo> astropathBriefs = AstropathBriefs;
		if (astropathBriefs != null)
		{
			for (int n = 0; n < astropathBriefs.Count; n++)
			{
				Hash128 val6 = ClassHasher<AstropathBriefInfo>.GetHash128(astropathBriefs[n]);
				result.Append(ref val6);
			}
		}
		Hash128 val7 = DictionaryBlueprintToListOfBlueprintsHasher<BlueprintStarSystemMap, BlueprintPointOfInterest>.GetHash128(InteractedPoints);
		result.Append(ref val7);
		Hash128 val8 = DictionaryBlueprintToListOfBlueprintsHasher<BlueprintStarSystemMap, BlueprintAnomaly>.GetHash128(InteractedAnomalies);
		result.Append(ref val8);
		Dictionary<BlueprintPlanet, BlueprintPlanetPrefab> planetChangedVisualPrefabs = PlanetChangedVisualPrefabs;
		if (planetChangedVisualPrefabs != null)
		{
			int val9 = 0;
			foreach (KeyValuePair<BlueprintPlanet, BlueprintPlanetPrefab> item in planetChangedVisualPrefabs)
			{
				Hash128 hash = default(Hash128);
				Hash128 val10 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item.Key);
				hash.Append(ref val10);
				Hash128 val11 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item.Value);
				hash.Append(ref val11);
				val9 ^= hash.GetHashCode();
			}
			result.Append(ref val9);
		}
		return result;
	}
}
