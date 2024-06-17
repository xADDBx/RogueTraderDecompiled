using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;

namespace Kingmaker.Globalmap.Colonization;

public static class ColoniesGenerator
{
	public static List<BlueprintColonyEvent> RandomizeEventsForColony(BlueprintPlanet planet, int? overrideEventCountInColony = null)
	{
		BlueprintColonyEventsRoot blueprintColonyEventsRoot = BlueprintWarhammerRoot.Instance.ColonyRoot.ColonyEvents.Get();
		List<BlueprintColonyEventsRoot.ColonyEventToTimer> list = new List<BlueprintColonyEventsRoot.ColonyEventToTimer>();
		List<BlueprintColonyEventsRoot.ColonyEventToTimer> list2 = new List<BlueprintColonyEventsRoot.ColonyEventToTimer>();
		HashSet<BlueprintColonyEvent> hashSet = new HashSet<BlueprintColonyEvent>();
		foreach (ColoniesState.ColonyData colony in Game.Instance.Player.ColoniesState.Colonies)
		{
			foreach (BlueprintColonyEventsRoot.ColonyEventToTimer item in colony.Colony.AllEventsForColony)
			{
				hashSet.Add(item.ColonyEvent);
			}
		}
		BlueprintColonyEventsRoot.ColonyEventToTimer[] events = blueprintColonyEventsRoot.Events;
		foreach (BlueprintColonyEventsRoot.ColonyEventToTimer colonyEventToTimer in events)
		{
			if (!hashSet.Contains(colonyEventToTimer.ColonyEvent) && colonyEventToTimer.ColonyEvent.IsAllowedOnPlanet(planet))
			{
				if (colonyEventToTimer.ColonyEvent.IsExclusiveForPlanet(planet))
				{
					list2.Add(colonyEventToTimer);
				}
				else
				{
					list.Add(colonyEventToTimer);
				}
			}
		}
		PersistentRandom.Generator generator = PersistentRandom.Seed(Game.Instance.Player.GameId.GetHashCode()).MakeGenerator("generate_colony_events", (Game.Instance.Player.WarpTravelState.WarpTravelsCount + 1) * (Game.Instance.Player.ColoniesState.Colonies.Count + 1));
		List<BlueprintColonyEvent> list3 = list2.Select((BlueprintColonyEventsRoot.ColonyEventToTimer x) => x.ColonyEvent).ToList();
		int num = overrideEventCountInColony ?? blueprintColonyEventsRoot.EventCountInColony;
		num -= list3.Count;
		for (int j = 0; j < num; j++)
		{
			if (!list.Any())
			{
				break;
			}
			BlueprintColonyEventsRoot.ColonyEventToTimer colonyEventToTimer2 = list.Random(PFStatefulRandom.GlobalMap, ((PersistentRandom.Generator)generator).NextRange);
			if (colonyEventToTimer2 != null)
			{
				list3.Add(colonyEventToTimer2.ColonyEvent);
			}
			list.Remove(colonyEventToTimer2);
		}
		return list3;
	}

	public static void AddEventToColony(BlueprintColonyEvent colonyEvent, BlueprintPlanet planet)
	{
		List<(ColonyEventState, ColoniesState.ColonyData)> colonyEventStates = GetColonyEventStates(colonyEvent);
		if (colonyEventStates.Count != 0)
		{
			PFLog.Default.Error(string.Format("Can not add colony event {0} to {1}: event already added to colonies {2}", colonyEvent, planet, string.Join(",", colonyEventStates.Select(((ColonyEventState, ColoniesState.ColonyData) x) => x.Item2))));
			return;
		}
		ColoniesState.ColonyData colonyData = Game.Instance.Player.ColoniesState.Colonies.FindOrDefault((ColoniesState.ColonyData x) => x.Planet == planet);
		if (colonyData == null)
		{
			PFLog.Default.Error($"Can not add colony event {colonyEvent} to {planet}: planet not colonized yet");
		}
		else
		{
			colonyData.Colony.AddEvent(colonyEvent);
		}
	}

	public static void RemoveEventFromColonies(BlueprintColonyEvent colonyEvent, bool addToExclusivePlanet = false, bool replaceWithOtherEvent = false, bool removeFromAllowedPlanet = false)
	{
		List<(ColonyEventState, ColoniesState.ColonyData)> colonyEventStates = GetColonyEventStates(colonyEvent);
		if (colonyEventStates.Count == 0)
		{
			PFLog.Default.Error($"Can not remove colony event {colonyEvent} : no colony with event");
			return;
		}
		foreach (var item2 in colonyEventStates)
		{
			ColoniesState.ColonyData item = item2.Item2;
			if (!colonyEvent.IsAllowedOnPlanet(item.Planet) || removeFromAllowedPlanet)
			{
				item.Colony.RemoveEvent(colonyEvent);
			}
		}
		if (colonyEvent.IsExclusive && addToExclusivePlanet)
		{
			ColoniesState.ColonyData colonyData = Game.Instance.Player.ColoniesState.Colonies.FindOrDefault((ColoniesState.ColonyData x) => x.Planet == colonyEvent.ExclusivePlanet);
			if (colonyData == null)
			{
				PFLog.Default.Log($"Event {colonyEvent} was not added automatically to the colony {colonyEvent.ExclusivePlanet} : planet not colonized yet");
			}
			else if (colonyData.Colony.GetEventState(colonyEvent) != 0)
			{
				PFLog.Default.Log($"Event {colonyEvent} was not added automatically to the colony {colonyEvent.ExclusivePlanet} : event already in colony events list");
			}
			else
			{
				colonyData.Colony.AddEvent(colonyEvent);
			}
		}
		foreach (var (colonyEventState, colonyData2) in colonyEventStates)
		{
			if (colonyEventState != ColonyEventState.Finished && replaceWithOtherEvent && colonyData2.Colony.GetEventState(colonyEvent) == ColonyEventState.None)
			{
				BlueprintColonyEvent blueprintColonyEvent = RandomizeEventsForColony(colonyData2.Planet, 1).FirstOrDefault();
				if (blueprintColonyEvent == null)
				{
					PFLog.Default.Warning($"Can not replace colony event {colonyEvent} with other: no available events");
				}
				else
				{
					colonyData2.Colony.AddEvent(blueprintColonyEvent, colonyEventState == ColonyEventState.Started);
				}
			}
		}
	}

	public static List<(ColonyEventState, ColoniesState.ColonyData)> GetColonyEventStates(BlueprintColonyEvent colonyEvent)
	{
		List<(ColonyEventState, ColoniesState.ColonyData)> list = new List<(ColonyEventState, ColoniesState.ColonyData)>();
		foreach (ColoniesState.ColonyData colony in Game.Instance.Player.ColoniesState.Colonies)
		{
			ColonyEventState eventState = colony.Colony.GetEventState(colonyEvent);
			if (eventState != 0)
			{
				list.Add((eventState, colony));
			}
		}
		return list;
	}
}
