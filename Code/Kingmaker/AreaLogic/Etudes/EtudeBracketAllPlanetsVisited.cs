using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("2f983d0b114b4a9e9a83f3d3452315fe")]
public class EtudeBracketAllPlanetsVisited : EtudeBracketTrigger, IAreaHandler, ISubscriber, IAreaActivationHandler, IScanStarSystemObjectHandler, ISubscriber<StarSystemObjectEntity>, IHashable
{
	public class SavableData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public List<MapToPlanets> ScannedPlanets = new List<MapToPlanets>();

		[JsonProperty]
		public HashSet<BlueprintStarSystemMap> FullyExploredAreas = new HashSet<BlueprintStarSystemMap>();

		[JsonProperty]
		public HashSet<BlueprintStarSystemMap> AllAreasToExplore = new HashSet<BlueprintStarSystemMap>();

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			List<MapToPlanets> scannedPlanets = ScannedPlanets;
			if (scannedPlanets != null)
			{
				for (int i = 0; i < scannedPlanets.Count; i++)
				{
					Hash128 val2 = ClassHasher<MapToPlanets>.GetHash128(scannedPlanets[i]);
					result.Append(ref val2);
				}
			}
			HashSet<BlueprintStarSystemMap> fullyExploredAreas = FullyExploredAreas;
			if (fullyExploredAreas != null)
			{
				int num = 0;
				foreach (BlueprintStarSystemMap item in fullyExploredAreas)
				{
					num ^= Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item).GetHashCode();
				}
				result.Append(num);
			}
			HashSet<BlueprintStarSystemMap> allAreasToExplore = AllAreasToExplore;
			if (allAreasToExplore != null)
			{
				int num2 = 0;
				foreach (BlueprintStarSystemMap item2 in allAreasToExplore)
				{
					num2 ^= Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item2).GetHashCode();
				}
				result.Append(num2);
			}
			return result;
		}
	}

	[SerializeField]
	private ActionList m_OnTriggerActions;

	[SerializeField]
	private BlueprintStarSystemMap.Reference[] m_ExcludeAreas;

	private List<BlueprintStarSystemMap> ExcludedAreas => m_ExcludeAreas?.Dereference().ToList();

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		OnAreaActive();
	}

	public void OnAreaActivated()
	{
		OnAreaActive();
	}

	private void OnAreaActive()
	{
		SavableData savableData = RequestSavableData<SavableData>();
		if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.GlobalMap && savableData.AllAreasToExplore.Empty())
		{
			savableData.AllAreasToExplore = (from area in Game.Instance.State.SectorMapObjects.All.Select((SectorMapObjectEntity sectorMapObj) => sectorMapObj.StarSystemArea).Except(ExcludedAreas)
				where area != null
				select area).ToHashSet();
		}
		if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.GlobalMap)
		{
			CheckAllPlanets();
		}
	}

	private void CheckAllPlanets()
	{
		SavableData savableData = RequestSavableData<SavableData>();
		if (savableData.AllAreasToExplore.Empty())
		{
			return;
		}
		bool flag = true;
		foreach (BlueprintStarSystemMap area in savableData.AllAreasToExplore)
		{
			if (area == null)
			{
				continue;
			}
			IEnumerable<BlueprintPlanet> enumerable = area.Planets.Dereference();
			IEnumerable<BlueprintPlanet> source = Game.Instance.Player.StarSystemsState.ScannedPlanets.Where((PlanetExplorationInfo info) => info.StarSystemMap == area)?.Select((PlanetExplorationInfo info) => info.Planet);
			foreach (BlueprintPlanet item in enumerable)
			{
				if (!source.Contains(item))
				{
					flag = false;
				}
			}
		}
		if (flag)
		{
			m_OnTriggerActions?.Run();
		}
	}

	public void HandleStartScanningStarSystemObject()
	{
	}

	public void HandleScanStarSystemObject()
	{
		if (Game.Instance.CurrentlyLoadedArea is BlueprintStarSystemMap item && !ExcludedAreas.Contains(item) && EventInvokerExtensions.Entity is PlanetEntity)
		{
			CheckAllPlanets();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
