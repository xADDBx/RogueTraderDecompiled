using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Quests.Logic;
using Kingmaker.Designers;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.UI.Common;

public static class UIUtilitySpaceQuests
{
	public static List<QuestObjective> GetQuestsForSystem(SectorMapObject system)
	{
		return GetQuestsForSystemWithBlueprint(system.StarSystemBlueprint.StarSystemToTransit?.Get() as BlueprintStarSystemMap);
	}

	public static List<QuestObjective> GetQuestsForSystemWithBlueprint(BlueprintStarSystemMap blueprintSystem)
	{
		List<QuestObjective> list = new List<QuestObjective>();
		IEnumerable<BlueprintPlanet> enumerable = blueprintSystem?.Planets.EmptyIfNull().Dereference();
		IEnumerable<Quest> list2 = GameHelper.Quests.GetList();
		if (enumerable != null)
		{
			foreach (Quest item in list2)
			{
				bool flag = false;
				if (!item.IsActive)
				{
					continue;
				}
				foreach (QuestObjective objective in item.Objectives)
				{
					if (flag)
					{
						break;
					}
					if (!objective.IsActive || objective.Blueprint.GetComponent<RumourMapMarker>() != null)
					{
						continue;
					}
					foreach (BlueprintArea area in objective.Blueprint.Areas)
					{
						if (flag)
						{
							break;
						}
						foreach (BlueprintPlanet item2 in enumerable)
						{
							if (item2.ConnectedAreas.Contains(area))
							{
								list.Add(objective);
								flag = true;
								break;
							}
						}
					}
				}
			}
		}
		return list;
	}

	public static List<QuestObjective> GetQuestsForPlanet(BlueprintPlanet planet)
	{
		List<QuestObjective> list = new List<QuestObjective>();
		IEnumerable<Quest> list2 = GameHelper.Quests.GetList();
		if (planet != null)
		{
			foreach (Quest item in list2)
			{
				bool flag = false;
				if (!item.IsActive)
				{
					continue;
				}
				foreach (QuestObjective objective in item.Objectives)
				{
					if (flag)
					{
						break;
					}
					if (!objective.IsActive || objective.Blueprint.GetComponent<RumourMapMarker>() != null)
					{
						continue;
					}
					foreach (BlueprintArea area in objective.Blueprint.Areas)
					{
						if (flag)
						{
							break;
						}
						if (planet.ConnectedAreas.Contains(area))
						{
							list.Add(objective);
							flag = true;
							break;
						}
					}
				}
			}
		}
		return list;
	}

	public static List<QuestObjective> GetRumoursForSystem(SectorMapObject system)
	{
		return GetRumoursForSystemWithBlueprint(system.StarSystemBlueprint.StarSystemToTransit?.Get() as BlueprintStarSystemMap);
	}

	public static List<QuestObjective> GetRumoursForSystemWithBlueprint(BlueprintStarSystemMap blueprintSystem)
	{
		List<QuestObjective> list = new List<QuestObjective>();
		IEnumerable<BlueprintPlanet> enumerable = blueprintSystem?.Planets.EmptyIfNull().Dereference();
		IEnumerable<Quest> list2 = GameHelper.Quests.GetList();
		if (enumerable != null)
		{
			foreach (Quest item in list2)
			{
				bool flag = false;
				if (!item.IsActive)
				{
					continue;
				}
				foreach (QuestObjective objective in item.Objectives)
				{
					if (flag)
					{
						break;
					}
					if (!objective.IsActive || objective.Blueprint.GetComponent<RumourMapMarker>() == null)
					{
						continue;
					}
					foreach (BlueprintArea area in objective.Blueprint.Areas)
					{
						if (flag)
						{
							break;
						}
						foreach (BlueprintPlanet item2 in enumerable)
						{
							if (item2.ConnectedAreas.Contains(area))
							{
								list.Add(objective);
								flag = true;
								break;
							}
						}
					}
				}
			}
		}
		return list;
	}

	public static List<QuestObjective> GetRumoursForPlanet(BlueprintPlanet planet)
	{
		List<QuestObjective> list = new List<QuestObjective>();
		IEnumerable<Quest> list2 = GameHelper.Quests.GetList();
		if (planet != null)
		{
			foreach (Quest item in list2)
			{
				bool flag = false;
				if (!item.IsActive)
				{
					continue;
				}
				foreach (QuestObjective objective in item.Objectives)
				{
					if (flag)
					{
						break;
					}
					if (!objective.IsActive || objective.Blueprint.GetComponent<RumourMapMarker>() == null)
					{
						continue;
					}
					foreach (BlueprintArea area in objective.Blueprint.Areas)
					{
						if (flag)
						{
							break;
						}
						if (planet.ConnectedAreas.Contains(area))
						{
							list.Add(objective);
							flag = true;
							break;
						}
					}
				}
			}
		}
		return list;
	}

	public static List<QuestObjective> GetQuestsForSpaceSystem(BlueprintStarSystemMap system)
	{
		List<QuestObjective> list = new List<QuestObjective>();
		IEnumerable<Quest> list2 = GameHelper.Quests.GetList();
		if (system != null)
		{
			foreach (Quest item in list2)
			{
				bool flag = false;
				if (!item.IsActive)
				{
					continue;
				}
				foreach (QuestObjective objective in item.Objectives)
				{
					if (flag)
					{
						break;
					}
					if (!objective.IsActive || objective.Blueprint.GetComponent<RumourMapMarker>() != null)
					{
						continue;
					}
					foreach (BlueprintArea area in objective.Blueprint.Areas)
					{
						if (flag)
						{
							break;
						}
						if (system == area)
						{
							list.Add(objective);
							flag = true;
							break;
						}
					}
				}
			}
		}
		return list;
	}

	public static List<string> GetQuestsStringList(List<QuestObjective> questsList, List<QuestObjective> questsInSystemList)
	{
		List<string> list = new List<string>();
		int currentIndexQuestsList = 0;
		if (questsList != null && !questsList.Empty())
		{
			list.AddRange(questsList.Where((QuestObjective quest) => !string.IsNullOrWhiteSpace((!quest.Blueprint.IsAddendum) ? quest.Blueprint.GetTitile() : quest.ParentObjective?.Blueprint.GetTitile())).Select((QuestObjective quest, int index) => $"{++currentIndexQuestsList}. " + ((!quest.Blueprint.IsAddendum) ? quest.Blueprint.GetTitile() : quest.ParentObjective?.Blueprint.GetTitile())).ToList());
		}
		int startingIndexForSystemList = currentIndexQuestsList;
		if (questsInSystemList != null && !questsInSystemList.Empty())
		{
			list.AddRange(questsInSystemList.Where((QuestObjective quest) => !string.IsNullOrWhiteSpace((!quest.Blueprint.IsAddendum) ? quest.Blueprint.GetTitile() : quest.ParentObjective?.Blueprint.GetTitile())).Select((QuestObjective quest, int index) => $"{++startingIndexForSystemList}. " + ((!quest.Blueprint.IsAddendum) ? quest.Blueprint.GetTitile() : quest.ParentObjective?.Blueprint.GetTitile())).ToList());
		}
		return list;
	}
}
