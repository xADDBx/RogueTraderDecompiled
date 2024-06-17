using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[TypeId("404e5d9170fb45ceaa82439aa523d696")]
public class EtudeBracketVisitAllSystems : EtudeBracketTrigger, IAreaHandler, ISubscriber, IAreaActivationHandler, IHashable
{
	public class SavableData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public HashSet<BlueprintStarSystemMap> VisitedAreas = new HashSet<BlueprintStarSystemMap>();

		[JsonProperty]
		public HashSet<BlueprintStarSystemMap> AllAreasToVisit = new HashSet<BlueprintStarSystemMap>();

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			HashSet<BlueprintStarSystemMap> visitedAreas = VisitedAreas;
			if (visitedAreas != null)
			{
				int num = 0;
				foreach (BlueprintStarSystemMap item in visitedAreas)
				{
					num ^= Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item).GetHashCode();
				}
				result.Append(num);
			}
			HashSet<BlueprintStarSystemMap> allAreasToVisit = AllAreasToVisit;
			if (allAreasToVisit != null)
			{
				int num2 = 0;
				foreach (BlueprintStarSystemMap item2 in allAreasToVisit)
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

	private bool CheckTrigger()
	{
		SavableData savableData = RequestSavableData<SavableData>();
		bool result = true;
		foreach (BlueprintStarSystemMap item in savableData.AllAreasToVisit)
		{
			if (item != null && !savableData.VisitedAreas.Contains(item))
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private void OnAreaActive()
	{
		SavableData savableData = RequestSavableData<SavableData>();
		if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.GlobalMap && savableData.AllAreasToVisit.Empty())
		{
			savableData.AllAreasToVisit = (from area in Game.Instance.State.SectorMapObjects.All.Select((SectorMapObjectEntity sectorMapObj) => sectorMapObj.StarSystemArea).Except(ExcludedAreas)
				where area != null
				select area).ToHashSet();
		}
		if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.StarSystem)
		{
			BlueprintStarSystemMap item = Game.Instance.CurrentlyLoadedArea as BlueprintStarSystemMap;
			if (!savableData.VisitedAreas.Contains(item))
			{
				savableData.VisitedAreas.Add(item);
			}
			if (!savableData.AllAreasToVisit.Empty() && CheckTrigger())
			{
				m_OnTriggerActions?.Run();
			}
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
