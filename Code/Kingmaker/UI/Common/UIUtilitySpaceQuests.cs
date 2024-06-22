using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Quests.Logic;
using Kingmaker.Designers;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Blueprints.SectorMap;
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
		IEnumerable<BlueprintStarSystemObject> obj = blueprintSystem?.Planets.EmptyIfNull().Dereference().Select((Func<BlueprintPlanet, BlueprintStarSystemObject>)((BlueprintPlanet planet) => planet));
		IEnumerable<BlueprintStarSystemObject> list2 = blueprintSystem?.OtherObjects?.EmptyIfNull().Dereference().Select((Func<BlueprintArtificialObject, BlueprintStarSystemObject>)((BlueprintArtificialObject planet) => planet));
		IEnumerable<BlueprintStarSystemObject> enumerable = Enumerable.Concat(second: ((from anomaly in blueprintSystem?.Anomalies?.EmptyIfNull().Dereference()
			where anomaly.ShowOnGlobalMap
			select anomaly).Select((Func<BlueprintAnomaly, BlueprintStarSystemObject>)((BlueprintAnomaly planet) => planet))).EmptyIfNull(), first: obj.EmptyIfNull().Concat(list2.EmptyIfNull()));
		IEnumerable<Quest> list3 = GameHelper.Quests.GetList();
		if (obj != null)
		{
			foreach (Quest item in list3)
			{
				bool flag = false;
				if (!item.IsActive)
				{
					continue;
				}
				QuestState state = item.State;
				if (state == QuestState.Completed || state == QuestState.Failed)
				{
					continue;
				}
				foreach (QuestObjective objective in item.Objectives)
				{
					if (flag)
					{
						break;
					}
					if (!objective.IsActive)
					{
						continue;
					}
					QuestObjectiveState state2 = objective.State;
					if (state2 == QuestObjectiveState.Completed || state2 == QuestObjectiveState.Failed || objective.Blueprint.GetComponent<RumourMapMarker>() != null)
					{
						continue;
					}
					foreach (BlueprintArea area in objective.Blueprint.Areas)
					{
						if (flag)
						{
							break;
						}
						foreach (BlueprintStarSystemObject item2 in enumerable)
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
				QuestState state = item.State;
				if (state == QuestState.Completed || state == QuestState.Failed)
				{
					continue;
				}
				foreach (QuestObjective objective in item.Objectives)
				{
					if (flag)
					{
						break;
					}
					if (!objective.IsActive)
					{
						continue;
					}
					QuestObjectiveState state2 = objective.State;
					if (state2 == QuestObjectiveState.Completed || state2 == QuestObjectiveState.Failed || objective.Blueprint.GetComponent<RumourMapMarker>() != null)
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

	public static List<QuestObjective> GetQuestsForAnomaly(BlueprintAnomaly anomaly)
	{
		List<QuestObjective> list = new List<QuestObjective>();
		IEnumerable<Quest> list2 = GameHelper.Quests.GetList();
		if (anomaly != null)
		{
			foreach (Quest item in list2)
			{
				bool flag = false;
				if (!item.IsActive)
				{
					continue;
				}
				QuestState state = item.State;
				if (state == QuestState.Completed || state == QuestState.Failed)
				{
					continue;
				}
				foreach (QuestObjective objective in item.Objectives)
				{
					if (flag)
					{
						break;
					}
					if (!objective.IsActive)
					{
						continue;
					}
					QuestObjectiveState state2 = objective.State;
					if (state2 == QuestObjectiveState.Completed || state2 == QuestObjectiveState.Failed || objective.Blueprint.GetComponent<RumourMapMarker>() != null)
					{
						continue;
					}
					foreach (BlueprintArea area in objective.Blueprint.Areas)
					{
						if (flag)
						{
							break;
						}
						if (anomaly.ConnectedAreas.Contains(area))
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
				QuestState state = item.State;
				if (state == QuestState.Completed || state == QuestState.Failed)
				{
					continue;
				}
				foreach (QuestObjective objective in item.Objectives)
				{
					if (flag)
					{
						break;
					}
					if (!objective.IsActive)
					{
						continue;
					}
					QuestObjectiveState state2 = objective.State;
					if (state2 == QuestObjectiveState.Completed || state2 == QuestObjectiveState.Failed || objective.Blueprint.GetComponent<RumourMapMarker>() == null)
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

	public static List<QuestObjective> GetRumoursForSectorMap(SectorMapObject system)
	{
		return GetRumoursForSectorMapWithBlueprint(system.StarSystemBlueprint);
	}

	public static List<QuestObjective> GetRumoursForSectorMapWithBlueprint(BlueprintSectorMapPointStarSystem blueprintSystem)
	{
		List<QuestObjective> list = new List<QuestObjective>();
		foreach (Quest item in GameHelper.Quests.GetList())
		{
			if (!item.IsActive)
			{
				continue;
			}
			QuestState state = item.State;
			if (state == QuestState.Completed || state == QuestState.Failed)
			{
				continue;
			}
			foreach (QuestObjective objective in item.Objectives)
			{
				if (!objective.IsActive)
				{
					continue;
				}
				QuestObjectiveState state2 = objective.State;
				if (state2 != QuestObjectiveState.Completed && state2 != QuestObjectiveState.Failed)
				{
					RumourMapMarker component = objective.Blueprint.GetComponent<RumourMapMarker>();
					if (component != null && component.SectorMapPointsToVisit.Dereference().Contains(blueprintSystem))
					{
						list.Add(objective);
						break;
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
				QuestState state = item.State;
				if (state == QuestState.Completed || state == QuestState.Failed)
				{
					continue;
				}
				foreach (QuestObjective objective in item.Objectives)
				{
					if (flag)
					{
						break;
					}
					if (!objective.IsActive)
					{
						continue;
					}
					QuestObjectiveState state2 = objective.State;
					if (state2 == QuestObjectiveState.Completed || state2 == QuestObjectiveState.Failed || objective.Blueprint.GetComponent<RumourMapMarker>() == null)
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
				QuestState state = item.State;
				if (state == QuestState.Completed || state == QuestState.Failed)
				{
					continue;
				}
				foreach (QuestObjective objective in item.Objectives)
				{
					if (flag)
					{
						break;
					}
					if (!objective.IsActive)
					{
						continue;
					}
					QuestObjectiveState state2 = objective.State;
					if (state2 == QuestObjectiveState.Completed || state2 == QuestObjectiveState.Failed || objective.Blueprint.GetComponent<RumourMapMarker>() != null)
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
		if (questsList != null && !questsList.Empty())
		{
			list.AddRange((from quest in questsList
				where !string.IsNullOrWhiteSpace((!quest.Blueprint.IsAddendum) ? quest.Blueprint.GetTitile().Text : quest.ParentObjective?.Blueprint.GetTitile().Text)
				select (!quest.Blueprint.IsAddendum) ? quest.Blueprint.GetTitile().Text : quest.ParentObjective?.Blueprint.GetTitile().Text).ToList());
		}
		if (questsInSystemList != null && !questsInSystemList.Empty())
		{
			list.AddRange((from quest in questsInSystemList
				where !string.IsNullOrWhiteSpace((!quest.Blueprint.IsAddendum) ? quest.Blueprint.GetTitile() : quest.ParentObjective?.Blueprint.GetTitile())
				select (!quest.Blueprint.IsAddendum) ? quest.Blueprint.GetTitile().Text : quest.ParentObjective?.Blueprint.GetTitile().Text).ToList());
		}
		list = list.Distinct().ToList();
		return list.Select((string str, int index) => $"{index + 1}. {str}").ToList();
	}
}
