using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Commands;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Controllers.StarSystem;

public class StarSystemAiController : BaseUnitController
{
	protected override void TickOnUnit(AbstractUnitEntity unit)
	{
		StarshipEntity playerShip = Game.Instance.Player.PlayerShip;
		if (unit == playerShip || playerShip == null || !unit.IsEnemy(Game.Instance.Player.PlayerShip) || !unit.IsInCombat)
		{
			return;
		}
		if (unit.DistanceTo(playerShip) > 2f)
		{
			if (unit.View.MovementAgent.WantsToMove)
			{
				return;
			}
			PathfindingService.Instance.FindPathRT_Delayed(unit.MovementAgent, Game.Instance.Player.PlayerShip.Position, 0.3f, 1, delegate(ForcedPath path)
			{
				if (path.error)
				{
					PFLog.Pathfinding.Error("An error path was returned. Ignoring");
				}
				else
				{
					UnitMoveToParams unitMoveToParams = new UnitMoveToParams(path, Game.Instance.Player.PlayerShip);
					float speedOnStarSystemMap = (unit.Blueprint as BlueprintStarship).SpeedOnStarSystemMap;
					unitMoveToParams.OverrideSpeed = speedOnStarSystemMap / StarSystemTimeController.TimeMultiplier;
					unit.Commands.Run(unitMoveToParams);
				}
			});
		}
		else if (Game.Instance.CurrentlyLoadedArea is BlueprintStarSystemMap { SpaceCombatArea: not null } blueprintStarSystemMap)
		{
			unit.LifeState.MarkedForDeath = true;
			UnitLifeController.ForceTickOnUnit(unit);
			Game.Instance.LoadArea(blueprintStarSystemMap.SpaceCombatArea, AutoSaveMode.None);
		}
	}
}
