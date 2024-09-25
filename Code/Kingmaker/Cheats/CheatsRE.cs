using Core.Cheats;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Globalmap.Blueprints.CombatRandomEncounters;

namespace Kingmaker.Cheats;

public static class CheatsRE
{
	public static int? RandomSeed;

	[Cheat(Name = "start_re", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void StartRE(string areaBlueprintName, string areaEnterPointBlueprintName = null, string enemyGroupBlueprintName = null)
	{
		BlueprintArea blueprint = Utilities.GetBlueprint<BlueprintArea>(areaBlueprintName);
		if (blueprint == null)
		{
			PFLog.Default.Log("Cannot find area with " + areaEnterPointBlueprintName);
			return;
		}
		BlueprintAreaEnterPoint bp = ((areaEnterPointBlueprintName != null) ? Utilities.GetBlueprint<BlueprintAreaEnterPoint>(areaEnterPointBlueprintName) : null);
		BlueprintRandomGroupOfUnits bp2 = ((enemyGroupBlueprintName != null) ? Utilities.GetBlueprint<BlueprintRandomGroupOfUnits>(enemyGroupBlueprintName) : null);
		Game.Instance.CombatRandomEncounterController.ActivateCombatRandomEncounter(blueprint.ToReference<BlueprintAreaReference>(), bp.ToReference<BlueprintAreaEnterPointReference>(), bp2.ToReference<BlueprintRandomGroupOfUnits.Reference>());
		BlueprintAreaEnterPoint enterPoint = Game.Instance.Player.CombatRandomEncounterState.EnterPoint;
		if (enterPoint.Area == Game.Instance.CurrentlyLoadedArea)
		{
			Game.Instance.Teleport(enterPoint);
		}
		else
		{
			Game.Instance.LoadArea(enterPoint, AutoSaveMode.None);
		}
	}

	[Cheat(Name = "turn_off_re", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void TurnOffRandomEncounters()
	{
		Game.Instance.Player.WarpTravelState.ForbidRE.Retain();
	}

	[Cheat(Name = "turn_on_re", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void TurnOnRandomEncounters()
	{
		Game.Instance.Player.WarpTravelState.ForbidRE.ReleaseAll();
	}

	[Cheat(Name = "set_random_seed", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SetRandomSeed(int seed)
	{
		RandomSeed = seed;
	}
}
