using System.Collections.Generic;
using System.Linq;
using Kingmaker.AI;
using Kingmaker.AI.BehaviourTrees;
using Kingmaker.AI.BehaviourTrees.Nodes;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Pathfinding;
using UnityEngine;

namespace Warhammer.SpaceCombat.AI.BehaviourTrees;

public class TaskNodeDoNextAction : TaskNode
{
	protected override Status TickInternal(Blackboard blackboard)
	{
		SpaceCombatDecisionContext spaceCombatDecisionContext = (SpaceCombatDecisionContext)blackboard.DecisionContext;
		spaceCombatDecisionContext.CurrentPathNode = spaceCombatDecisionContext.BestPath.FirstOrDefault();
		GraphNode node = AstarPath.active.GetNearest(spaceCombatDecisionContext.Unit.Position).node;
		int num = CustomGraphHelper.GuessDirection(spaceCombatDecisionContext.Unit.Forward);
		if (spaceCombatDecisionContext.CurrentPathNode == null || spaceCombatDecisionContext.CurrentPathNode.node != node || spaceCombatDecisionContext.CurrentPathNode.direction != num)
		{
			AILogger.Instance.Error(new AILogReason(AILogReasonType.StarshipIsOffCource));
			return Status.Failure;
		}
		UnitCommandParams unitCommandParams = CreateCommandParams(spaceCombatDecisionContext);
		if (unitCommandParams != null)
		{
			spaceCombatDecisionContext.IsLastActionBrokePlan = IsCommandBreaksPlan(spaceCombatDecisionContext, unitCommandParams);
			spaceCombatDecisionContext.Unit.Commands.Run(unitCommandParams);
			return Status.Success;
		}
		return Status.Failure;
	}

	private UnitCommandParams CreateCommandParams(SpaceCombatDecisionContext context)
	{
		Ability abilityToCast = GetAbilityToCast(context);
		if (abilityToCast == null)
		{
			return CreateMoveCommandParams(context);
		}
		return CreateCastAbilityCommandParams(context, abilityToCast);
	}

	private Ability GetAbilityToCast(SpaceCombatDecisionContext context)
	{
		if (!context.PathNodesWithAbilities.TryGetValue(context.CurrentPathNode, out var value))
		{
			return null;
		}
		if (value == null || value.Count == 0)
		{
			context.PathNodesWithAbilities.Remove(context.CurrentPathNode);
			return null;
		}
		value.Sort((Ability a, Ability b) => AbilityBreaksPlan(b.Blueprint).CompareTo(AbilityBreaksPlan(a.Blueprint)));
		Ability result = value.LastItem();
		value.RemoveLast();
		return result;
	}

	private UnitCommandParams CreateCastAbilityCommandParams(SpaceCombatDecisionContext context, Ability ability)
	{
		UnitCommandParams unitCommandParams = null;
		if (IsSummoningUnitAbility(ability))
		{
			Vector3 vector = context.Unit.Position + context.Unit.Forward;
			GraphNode summonNode = AstarPath.active.GetNearest(vector).node;
			context.BestPath.ForEach(delegate(ShipPath.DirectionalPathNode p)
			{
				if (p.node == summonNode)
				{
					p.canStand = false;
				}
			});
			unitCommandParams = new UnitUseAbilityParams(ability.Data, new TargetWrapper(vector));
		}
		else if (IsSelfTargetAbility(ability))
		{
			unitCommandParams = new UnitUseAbilityParams(ability.Data, new TargetWrapper(context.Unit));
		}
		else
		{
			TargetWrapper targetWrapper = ChooseTarget(context, ability);
			if ((object)targetWrapper != null)
			{
				unitCommandParams = new UnitUseAbilityParams(ability.Data, targetWrapper);
			}
		}
		if (unitCommandParams == null)
		{
			return null;
		}
		AILogger.Instance.Log(AILogAbility.Cast(ability.Data, unitCommandParams.Target));
		return unitCommandParams;
	}

	private bool IsSummoningUnitAbility(Ability ab)
	{
		AbilityEffectRunAction component = ab.GetComponent<AbilityEffectRunAction>();
		if (!ab.GetComponent<AbilityCustomStarshipNPCTorpedoLaunch>())
		{
			return component?.Actions.Actions.Any((GameAction a) => a is WarhammerContextActionSpawnChildStarship) ?? false;
		}
		return true;
	}

	private bool IsSelfTargetAbility(Ability ability)
	{
		if (!ability.Blueprint.CanTargetEnemies)
		{
			return ability.Blueprint.CanTargetSelf;
		}
		return false;
	}

	private TargetWrapper ChooseTarget(SpaceCombatDecisionContext context, Ability ability)
	{
		PartUnitBrain brain = context.Unit.Brain;
		ShipPath.DirectionalPathNode currentPathNode = context.CurrentPathNode;
		int value = context.AbilityValueCache.GetValue(currentPathNode, ability);
		List<TargetInfo> targets = context.Enemies;
		(brain?.Blueprint as BlueprintStarshipBrain)?.TryOverrideTargets(context, ref targets);
		foreach (TargetInfo item in targets)
		{
			TargetWrapper appropriateTarget = GetAppropriateTarget(ability.Data, item.Entity);
			if (ability.Data.CanTargetFromNode(currentPathNode.node, null, appropriateTarget, out var _, out var _) && ability.Data.IsTargetInsideRestrictedFiringArc(appropriateTarget, currentPathNode.node, currentPathNode.direction) && brain.GetAbilityValue(ability.Data, item.Entity) == value)
			{
				return appropriateTarget;
			}
		}
		return null;
	}

	private UnitCommandParams CreateMoveCommandParams(SpaceCombatDecisionContext context)
	{
		List<GraphNode> list = new List<GraphNode>();
		ShipPath.DirectionalPathNode directionalPathNode = context.CurrentPathNode;
		List<ShipPath.DirectionalPathNode> toRemove = new List<ShipPath.DirectionalPathNode>();
		GraphNode graphNode = null;
		foreach (ShipPath.DirectionalPathNode item in context.BestPath)
		{
			list.Add(item.node);
			directionalPathNode = item;
			if (directionalPathNode.canStand)
			{
				graphNode = directionalPathNode.node;
				if (context.PathNodesWithAbilities.ContainsKey(item))
				{
					break;
				}
				toRemove.Add(item);
				continue;
			}
			break;
		}
		while (list.Count > 0)
		{
			if (list[list.Count - 1] == graphNode)
			{
				break;
			}
			list.RemoveLast();
		}
		if (list.Count == 0)
		{
			AILogger.Instance.Error(new AILogReason(AILogReasonType.PathIsEmpty));
			return null;
		}
		context.BestPath.RemoveAll((ShipPath.DirectionalPathNode p) => toRemove.Contains(p));
		ForcedPath forcedPath = ForcedPath.Construct(list.Select((GraphNode p) => p.Vector3Position).ToList(), list);
		int diagonalCount = directionalPathNode.diagonalCount;
		int straightDistance = directionalPathNode.straightDistance;
		int lengthFromStart = directionalPathNode.lengthFromStart;
		UnitMoveToProperParams result = new UnitMoveToProperParams(forcedPath, straightDistance, diagonalCount, lengthFromStart);
		AILogger.Instance.Log(AILogMovement.Move(forcedPath.path.Last()));
		return result;
	}

	private TargetWrapper GetAppropriateTarget(AbilityData ability, MechanicEntity enemy)
	{
		if (ability.Blueprint.GetComponent<AbilityCustomStarshipNPCTorpedoLaunch>() != null)
		{
			return new TargetWrapper(enemy.Position + 5 * enemy.SizeRect.Width * enemy.Forward.ToCellSizedVector());
		}
		return new TargetWrapper(enemy);
	}

	private bool IsCommandBreaksPlan(SpaceCombatDecisionContext context, UnitCommandParams cmd)
	{
		BlueprintAbility blueprintAbility = (cmd as UnitUseAbilityParams)?.Ability?.Blueprint;
		if (blueprintAbility == null)
		{
			return false;
		}
		if (AbilityBreaksPlan(blueprintAbility))
		{
			return true;
		}
		return context.Unit.Buffs.RawFacts.Where((Buff buf) => buf.Blueprint.GetComponent<StarshipAIBreakPlan>()).Any();
	}

	private bool AbilityBreaksPlan(BlueprintAbility bp)
	{
		if (bp.GetComponent<AbilityCustomTeleport>() == null)
		{
			return bp.GetComponent<StarshipAIBreakPlan>() != null;
		}
		return true;
	}
}
