using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View.Covers;
using Pathfinding;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartJumpAsideDodge : UnitPart, IEntitySubscriber, ITurnEndHandler<EntitySubscriber>, ITurnEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<ITurnEndHandler, EntitySubscriber>, IHashable
{
	public int SpentMovePoints { get; private set; }

	public static bool NeedStepAsideDodge(UnitEntity unit, RulePerformDodge dodgeRoll)
	{
		if (!dodgeRoll.IsMelee && dodgeRoll.Ability.IsAOE && dodgeRoll.CoverType == LosCalculations.CoverType.None)
		{
			return !unit.HasMechanicFeature(MechanicsFeatureType.AoEDodgeWithoutMovement);
		}
		return false;
	}

	public static bool ShouldDodge(UnitEntity unit, RulePerformDodge dodgeRoll)
	{
		if (dodgeRoll.Defender != unit)
		{
			return false;
		}
		if (Game.Instance.TurnController.CurrentUnit == unit)
		{
			return false;
		}
		return true;
	}

	public static bool CanDodge(UnitEntity unit, RulePerformDodge dodgeRule, [CanBeNull] out ForcedPath safePath, out int pathCost)
	{
		if (!unit.State.CanDodgeWithMove)
		{
			safePath = null;
			pathCost = 0;
			return false;
		}
		int result = Rulebook.Trigger(new RuleCalculateMovementPoints(unit)).Result;
		return FindClosestSafePath(unit, dodgeRule, result, out safePath, out pathCost);
	}

	public void Dodge(ForcedPath safePath, int pathCost)
	{
		SpentMovePoints += pathCost;
		UnitJumpAsideDodgeParams unitJumpAsideDodgeParams = new UnitJumpAsideDodgeParams(safePath);
		unitJumpAsideDodgeParams.ApproachRadius = safePath?.LengthInCells() ?? 1;
		base.Owner.Commands.Run(unitJumpAsideDodgeParams);
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		SpentMovePoints = 0;
	}

	private static bool FindClosestSafePath(UnitEntity unit, RulePerformDodge dodgeRule, int availableMovePoints, [CanBeNull] out ForcedPath safePath, out int pathCost)
	{
		if (dodgeRule.Ability.IsScatter)
		{
			return FindClosestSafePathFromScatter(unit, dodgeRule.Attacker, availableMovePoints, out safePath, out pathCost);
		}
		return FindClosestSafePathFromAOE(unit, dodgeRule.DangerArea, availableMovePoints, out safePath, out pathCost);
	}

	private static bool FindClosestSafePathFromAOE(UnitEntity unit, HashSet<CustomGridNodeBase> dangerArea, int availableMovePoints, out ForcedPath safePath, out int pathCost)
	{
		safePath = null;
		pathCost = int.MaxValue;
		CustomGridNodeBase customGridNodeBase = (CustomGridNodeBase)AstarPath.active.GetNearest(unit.Position).node;
		Dictionary<GraphNode, WarhammerPathPlayerCell> dictionary = PathfindingService.Instance.FindAllReachableTiles_Blocking(unit.View.MovementAgent, unit.Position, availableMovePoints, ignoreThreateningAreaCost: true);
		if (dictionary.Count == 0)
		{
			return false;
		}
		IOrderedEnumerable<KeyValuePair<GraphNode, WarhammerPathPlayerCell>> orderedEnumerable = dictionary.OrderBy((KeyValuePair<GraphNode, WarhammerPathPlayerCell> x) => x.Value.Length);
		List<GraphNode> list = new List<GraphNode>();
		foreach (KeyValuePair<GraphNode, WarhammerPathPlayerCell> item in orderedEnumerable)
		{
			GraphNode key = item.Key;
			if (key != null && key != customGridNodeBase && item.Value.IsCanStand && !dangerArea.Contains(key))
			{
				if ((int)item.Value.Length > pathCost)
				{
					break;
				}
				pathCost = (int)item.Value.Length;
				list.Add(key);
			}
		}
		if (list.Count == 0)
		{
			return false;
		}
		GraphNode node = list[unit.Random.Range(0, list.Count)];
		safePath = WarhammerPathHelper.ConstructPathTo(node, dictionary);
		if (safePath.path != null)
		{
			return safePath.path.Count != 0;
		}
		return false;
	}

	private static bool FindClosestSafePathFromScatter(UnitEntity unit, MechanicEntity attacker, int availableMovePoints, out ForcedPath safePath, out int pathCost)
	{
		safePath = null;
		pathCost = int.MaxValue;
		if (availableMovePoints == 0)
		{
			return false;
		}
		CustomGridNodeBase origin = (CustomGridNodeBase)AstarPath.active.GetNearest(unit.Position).node;
		Dictionary<GraphNode, WarhammerPathPlayerCell> dictionary = PathfindingService.Instance.FindAllReachableTiles_Blocking(unit.View.MovementAgent, unit.Position, availableMovePoints, ignoreThreateningAreaCost: true);
		if (dictionary.Count == 0)
		{
			return false;
		}
		List<GraphNode> list = new List<GraphNode>();
		CustomGridNodeBase[] array;
		CustomGridNodeBase[] array2 = (array = new CustomGridNodeBase[2]);
		(CustomGridNodeBase, CustomGridNodeBase) orthoNeighbours = LosCalculations.GetOrthoNeighbours(origin, (unit.Position - attacker.Position).normalized);
		array[0] = orthoNeighbours.Item1;
		array2[1] = orthoNeighbours.Item2;
		array = array2;
		foreach (CustomGridNodeBase customGridNodeBase in array)
		{
			if (customGridNodeBase != null && dictionary.TryGetValue(customGridNodeBase, out var value) && value.IsCanStand)
			{
				list.Add(customGridNodeBase);
			}
		}
		if (list.Count == 0)
		{
			return false;
		}
		GraphNode node = list[unit.Random.Range(0, list.Count)];
		safePath = WarhammerPathHelper.ConstructPathTo(node, dictionary);
		if (safePath.path != null)
		{
			return safePath.path.Count != 0;
		}
		return false;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
