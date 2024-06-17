using System.Linq;
using Core.Cheats;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.SystemMap;

namespace Kingmaker.Cheats;

internal static class CheatsColonization
{
	[Cheat(Name = "colonize", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ColonizePlanet(BlueprintPlanet blueprintPlanet)
	{
		PlanetEntity planetEntity = (Game.Instance.State.StarSystemObjects.FirstOrDefault((StarSystemObjectEntity sso) => sso.Blueprint == blueprintPlanet) as PlanetEntity) ?? new PlanetEntity(null, blueprintPlanet);
		planetEntity.IsScanned = true;
		Game.Instance.ColonizationController.Colonize(planetEntity, isPlayerCommand: false);
	}

	[Cheat(Name = "enable_colonization", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void EnableColonization()
	{
		Game.Instance.Player.ColoniesState.ForbidColonization.ReleaseAll();
	}

	[Cheat(Name = "finish_projects", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void FinishAllCurrentProjects()
	{
		foreach (ColoniesState.ColonyData colony in Game.Instance.Player.ColoniesState.Colonies)
		{
			foreach (ColonyProject project in colony.Colony.Projects)
			{
				if (!project.IsFinished)
				{
					colony.Colony.FinishProject(project);
				}
			}
		}
	}

	[Cheat(Name = "add_colony_resource", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void AddResourceToColoniesCheat(BlueprintResource resource, int resourceCount = 1)
	{
		if (resourceCount > 0)
		{
			Game.Instance.ColonizationController.AddResourceNotFromColonyToPool(resource, resourceCount);
		}
		else
		{
			Game.Instance.ColonizationController.UseResourceFromPool(resource, resourceCount);
		}
	}

	[Cheat(Name = "start_project", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void StartProject(BlueprintColony colonyBlueprint, BlueprintColonyProject project)
	{
		Colony colony = Game.Instance.Player.ColoniesState.Colonies.FirstOrDefault((ColoniesState.ColonyData state) => state.Colony.Blueprint == colonyBlueprint)?.Colony;
		if (colony == null)
		{
			PFLog.Default.Warning("No existing colony with blueprint " + colonyBlueprint.name);
		}
		else
		{
			colony.StartProject(project);
		}
	}

	[Cheat(Name = "use_miner", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void UseMiner(BlueprintResource resource)
	{
		StarSystemObjectEntity data = Game.Instance.StarSystemMapController.StarSystemShip.StarSystemObjectLandOn.Data;
		if (data != null && data.ResourcesOnObject.ContainsKey(resource))
		{
			Game.Instance.ColonizationController.UseResourceMiner(data, resource);
		}
		else
		{
			PFLog.Default.Log("sso is null or doesn't have current resource on it");
		}
	}

	[Cheat(Name = "remove_miner", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void RemoveMiner(BlueprintResource resource)
	{
		StarSystemObjectEntity data = Game.Instance.StarSystemMapController.StarSystemShip.StarSystemObjectLandOn.Data;
		if (data != null && data.ResourceMiners.ContainsKey(resource))
		{
			Game.Instance.ColonizationController.RemoveResourceMiner(data, resource);
		}
		else
		{
			PFLog.Default.Log("sso is null or doesn't have current resource on it");
		}
	}

	[Cheat(Name = "add_pf", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void AddPF(int value)
	{
		Game.Instance.Player.ProfitFactor.AddModifier(value);
	}

	[Cheat(Name = "add_colony_stat", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void AddColonyStat(BlueprintColony colonyBlueprint, string stat, int value)
	{
		ColoniesState.ColonyData colonyData = Game.Instance.Player.ColoniesState.Colonies.FirstOrDefault((ColoniesState.ColonyData state) => state.Colony.Blueprint == colonyBlueprint);
		if (colonyData == null)
		{
			PFLog.Default.Warning("No existing colony with blueprint " + colonyBlueprint.name);
			return;
		}
		if (stat == "contentment")
		{
			colonyData.Colony.Contentment.Modifiers.Add(new ColonyStatModifier
			{
				Value = value,
				Modifier = null,
				ModifierType = ColonyStatModifierType.Other
			});
		}
		if (stat == "efficiency")
		{
			colonyData.Colony.Efficiency.Modifiers.Add(new ColonyStatModifier
			{
				Value = value,
				Modifier = null,
				ModifierType = ColonyStatModifierType.Other
			});
		}
		if (stat == "security")
		{
			colonyData.Colony.Security.Modifiers.Add(new ColonyStatModifier
			{
				Value = value,
				Modifier = null,
				ModifierType = ColonyStatModifierType.Other
			});
		}
	}

	[Cheat(Name = "add_event_to_colony", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatAddEventToColony(BlueprintColonyEvent colonyEvent, BlueprintPlanet planet, bool ignoreColonyEventRequirements = false)
	{
		using (ContextData<IgnoreColonyEventRequirements>.RequestIf(ignoreColonyEventRequirements))
		{
			ColoniesGenerator.AddEventToColony(colonyEvent, planet);
		}
	}

	[Cheat(Name = "remove_event_from_colony", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatRemoveEventFromColony(BlueprintColonyEvent colonyEvent, bool replaceWithOtherEvent = false, bool addToExclusivePlanet = false, bool ignoreColonyEventRequirements = false, bool removeFromAllowedPlanet = false)
	{
		using (ContextData<IgnoreColonyEventRequirements>.RequestIf(ignoreColonyEventRequirements))
		{
			ColoniesGenerator.RemoveEventFromColonies(colonyEvent, addToExclusivePlanet, replaceWithOtherEvent, removeFromAllowedPlanet);
		}
	}

	[Cheat(Name = "log_colonies_events", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatLogColoniesEvents()
	{
		foreach (ColoniesState.ColonyData colony in Game.Instance.Player.ColoniesState.Colonies)
		{
			PFLog.Default.Log(string.Format("{0} : {1}", colony.Planet, string.Join(",", colony.Colony.AllEventsForColony.Select((BlueprintColonyEventsRoot.ColonyEventToTimer x) => x.ColonyEvent))));
		}
	}
}
