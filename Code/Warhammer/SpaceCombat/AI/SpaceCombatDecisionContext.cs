using System.Collections.Generic;
using Kingmaker.AI;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities;

namespace Warhammer.SpaceCombat.AI;

public class SpaceCombatDecisionContext : DecisionContext
{
	public AbilityValueCache AbilityValueCache;

	public ShipPath.DirectionalPathNode BestPathNode;

	public ShipPath.DirectionalPathNode CurrentPathNode;

	public List<ShipPath.DirectionalPathNode> BestPath = new List<ShipPath.DirectionalPathNode>();

	public bool IsBlockedByShip;

	public Dictionary<ShipPath.DirectionalPathNode, List<Ability>> PathNodesWithAbilities = new Dictionary<ShipPath.DirectionalPathNode, List<Ability>>();

	public bool IsLastActionBrokePlan;

	public float BestTrajectoryScore;
}
