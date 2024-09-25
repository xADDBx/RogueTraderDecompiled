using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("03a285e0b85a4b989bc9784990b62090")]
public class EtudeBracketExploreAllInSystems : EtudeBracketTrigger, IStarSystemMapResearchProgress, ISubscriber, IHashable
{
	public class SavableData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public HashSet<BlueprintStarSystemMap> ExploredAreas = new HashSet<BlueprintStarSystemMap>();

		[JsonProperty]
		public HashSet<BlueprintStarSystemMap> AllAreasToExplore = new HashSet<BlueprintStarSystemMap>();

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			HashSet<BlueprintStarSystemMap> exploredAreas = ExploredAreas;
			if (exploredAreas != null)
			{
				int num = 0;
				foreach (BlueprintStarSystemMap item in exploredAreas)
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

	[SerializeField]
	[Tooltip("For areas that are not on global map")]
	private BlueprintStarSystemMap.Reference[] m_AdditionalAreas;

	private IEnumerable<BlueprintStarSystemMap> ExcludedAreas => m_ExcludeAreas?.Dereference();

	private IEnumerable<BlueprintStarSystemMap> AdditionalAreas => m_AdditionalAreas?.Dereference();

	public void HandleResearchPercentRecalculate(BlueprintStarSystemMap areaBlueprint, float value)
	{
		SavableData savableData = RequestSavableData<SavableData>();
		if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.GlobalMap && savableData.AllAreasToExplore.Empty())
		{
			savableData.AllAreasToExplore = Game.Instance.State.SectorMapObjects.All.Select((SectorMapObjectEntity sectorMapObj) => sectorMapObj.StarSystemArea).Except(ExcludedAreas).ToHashSet();
			savableData.AllAreasToExplore.AddRange(AdditionalAreas);
		}
		if (Mathf.CeilToInt(value) >= 100 && Game.Instance.StarSystemMapController.IsResearchedFully(areaBlueprint))
		{
			savableData.ExploredAreas.Add(areaBlueprint);
			if (CheckTrigger())
			{
				m_OnTriggerActions?.Run();
			}
		}
	}

	private bool CheckTrigger()
	{
		SavableData savableData = RequestSavableData<SavableData>();
		bool result = true;
		foreach (BlueprintStarSystemMap item in savableData.AllAreasToExplore)
		{
			if (!savableData.ExploredAreas.Contains(item))
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
