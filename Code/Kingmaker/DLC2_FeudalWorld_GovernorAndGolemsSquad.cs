using System.Collections.Generic;
using System.Linq;
using Kingmaker.AI;
using Kingmaker.AI.BehaviourTrees;
using Kingmaker.AI.BehaviourTrees.Nodes;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker;

public class DLC2_FeudalWorld_GovernorAndGolemsSquad : ICustomBehaviourTreeBuilder
{
	private struct GovernorPathBuilder : Linecast.ICanTransitionBetweenCells
	{
		public List<GraphNode> Path;

		public bool CanTransitionBetweenCells(CustomGridNodeBase nodeFrom, CustomGridNodeBase nodeTo, Vector3 transitionPosition, float distanceFactor)
		{
			if (Path == null)
			{
				Path = new List<GraphNode>();
			}
			Path.Add(nodeFrom);
			return true;
		}
	}

	public BehaviourTree Create(MechanicEntity entity)
	{
		BaseUnitEntity leadingGolem = null;
		Sequence rootNode = new Sequence(new AsyncTaskNodeInitializeDecisionContext(), new Loop(InitEnumerationOverSquadUnits, (Blackboard b) => ConsiderNextSquadUnitExcept(b, GetGovernor(b)), new Sequence(new AsyncTaskNodeCreateMoveVariants(50), new TaskNodeExecute(StorePathToClosestEnemyForCurrentSquadUnit))), new TaskNodeExecute(delegate(Blackboard b)
		{
			leadingGolem = SelectLeadingGolem(b);
		}), new TaskNodeExecute(delegate(Blackboard b)
		{
			MakePreparatoryMoveOfLeadingGolem(b, leadingGolem);
		}), new TaskNodeWaitCommandsDone(), new TaskNodeExecute(delegate(Blackboard b)
		{
			PrepareSquadForMovementCalculations(b, leadingGolem);
		}), new AsyncTaskNodeCreateMoveVariants(50), new Succeeder(TaskNodeSetupMoveCommand.ToClosestEnemy()), new TaskNodeExecute(StoreMoveCommandForCurrentSquadUnit), new Loop(InitEnumerationOverSquadUnits, (Blackboard b) => ConsiderNextSquadUnitExcept(b, leadingGolem), new Sequence(new AsyncTaskNodeCreateMoveVariants(50), new Condition((Blackboard b) => b.DecisionContext.CurrentSquadUnit == GetGovernor(b), new TaskNodeExecute(delegate(Blackboard b)
		{
			SetupGovernorMoveCommand(b, leadingGolem);
		}), TaskNodeSetupMoveCommand.ToSquadLeader()), new TaskNodeExecute(StoreMoveCommandForCurrentSquadUnit))), new Loop(InitEnumerationOverSquadUnits, ConsiderNextSquadUnit, new Sequence(new TaskNodeExecuteMoveCommand(), new TaskNodeExecute(SpendAllMovePointsOfCurrentSquadUnit))), new TaskNodeWaitCommandsDone(), new TaskNodeTryFinishTurn());
		return new BehaviourTree(entity, rootNode, new DecisionContext());
	}

	private static void StorePathToClosestEnemyForCurrentSquadUnit(Blackboard blackboard)
	{
		DecisionContext decisionContext = blackboard.DecisionContext;
		SetupMoveCommandHelper.CreatePathToUnit(decisionContext, SetupMoveCommandMode.ClosestEnemy, out var path);
		if (path != null)
		{
			UnitMoveToProperParams item = new UnitMoveToProperParams(path, 0f);
			decisionContext.SquadUnitsMoveCommands.Add((decisionContext.CurrentSquadUnit, item));
		}
	}

	private static void PrepareSquadForMovementCalculations(Blackboard b, BaseUnitEntity leadingGolem)
	{
		DecisionContext decisionContext = b.DecisionContext;
		decisionContext.CurrentSquadUnit = leadingGolem;
		decisionContext.SquadUnitsMoveCommands.Clear();
		PartSquad squadOptional = leadingGolem.GetSquadOptional();
		if (squadOptional != null)
		{
			squadOptional.Squad.Units.ForEach(delegate(UnitReference x)
			{
				x.ToBaseUnitEntity().MovementAgent.Blocker.Unblock();
			});
		}
	}

	private static void MakePreparatoryMoveOfLeadingGolem(Blackboard blackboard, BaseUnitEntity leadingGolem)
	{
		BaseUnitEntity governor = GetGovernor(blackboard);
		(BaseUnitEntity, UnitMoveToProperParams) tuple = blackboard.DecisionContext.SquadUnitsMoveCommands.FirstOrDefault(((BaseUnitEntity unit, UnitMoveToProperParams cmd) x) => x.unit == leadingGolem);
		List<GraphNode> list = tuple.Item2?.ForcedPath?.path;
		if (list == null || list.Count <= 0)
		{
			return;
		}
		GraphNode graphNode = list[0];
		IntRect sizeRect = leadingGolem.SizeRect;
		int num = 0;
		for (int i = 1; i < list.Count; i++)
		{
			GraphNode graphNode2 = list[i];
			if (governor.DistanceToInCells(graphNode2.Vector3Position, sizeRect) < 2 && leadingGolem.CanStandHere(graphNode2))
			{
				graphNode = graphNode2;
				num = i;
			}
		}
		if (graphNode != list[0])
		{
			PFLog.AI.Log($"<Governor&Golems> Leading Golem makes a preparatory move to {graphNode}");
			UnitMoveToProperParams cmdParams = new UnitMoveToProperParams(ForcedPath.Construct(list.GetRange(0, num + 1)), 0f);
			leadingGolem.Commands.Run(cmdParams);
			UnitMoveToProperParams item = new UnitMoveToProperParams(ForcedPath.Construct(list.GetRange(num, list.Count - num)), 0f);
			tuple.Item2 = item;
		}
	}

	private static void SetupGovernorMoveCommand(Blackboard blackboard, BaseUnitEntity leadingGolem)
	{
		DecisionContext decisionContext = blackboard.DecisionContext;
		List<GraphNode> source = decisionContext.SquadUnitsMoveCommands.FirstOrDefault(((BaseUnitEntity unit, UnitMoveToProperParams cmd) x) => x.unit == leadingGolem).cmd?.ForcedPath?.path ?? TempList.Get<GraphNode>();
		CustomGridNodeBase nearestNodeXZ = GetGovernor(blackboard).GetNearestNodeXZ();
		Vector3 vector3Position = nearestNodeXZ.Vector3Position;
		NodeList occupiedNodes = GridAreaHelper.GetNodes(source.LastOrDefault()?.Vector3Position ?? leadingGolem.Position, leadingGolem.SizeRect);
		GovernorPathBuilder condition = default(GovernorPathBuilder);
		Linecast.LinecastGrid2(nearestNodeXZ.Graph, vector3Position, leadingGolem.Position, nearestNodeXZ, out var _, NNConstraint.None, ref condition);
		condition.Path.AddRange(source.Where((GraphNode n) => !occupiedNodes.Contains(n)));
		if (condition.Path.Count < 2)
		{
			PFLog.AI.Log("<Governor&Golems> Governor won't move");
			return;
		}
		UnitMoveToProperParams unitMoveToProperParams = new UnitMoveToProperParams(ForcedPath.Construct(condition.Path), 0f);
		decisionContext.SquadUnitsMoveCommands.Add((decisionContext.CurrentSquadUnit, unitMoveToProperParams));
		PFLog.AI.Log($"<Governor&Golems> Governor moves to {unitMoveToProperParams.ForcedPath.path.Last()}");
	}

	private static BaseUnitEntity SelectLeadingGolem(Blackboard b)
	{
		DecisionContext decisionContext = b.DecisionContext;
		BaseUnitEntity governor = GetGovernor(b);
		(BaseUnitEntity, UnitMoveToProperParams) item = decisionContext.SquadUnitsMoveCommands.Where(((BaseUnitEntity unit, UnitMoveToProperParams cmd) x) => x.unit != governor).MinBy(((BaseUnitEntity unit, UnitMoveToProperParams cmd) x) => Rulebook.Trigger(new RuleCalculateMovementCost(x.unit, x.cmd.ForcedPath)).ResultFullPathAPCost);
		decisionContext.SquadUnitsMoveCommands.Clear();
		decisionContext.SquadUnitsMoveCommands.Add(item);
		PFLog.AI.Log($"<Governor&Golems> Leading Golem is {item.Item1}");
		return item.Item1;
	}

	private static BaseUnitEntity GetGovernor(Blackboard b)
	{
		return b.DecisionContext.SquadLeader;
	}

	private static void InitEnumerationOverSquadUnits(Blackboard b)
	{
		b.DecisionContext.InitSquadUnitsEnumerator();
	}

	private static bool ConsiderNextSquadUnit(Blackboard b)
	{
		DecisionContext decisionContext = b.DecisionContext;
		decisionContext.ConsiderNextSquadUnit();
		return decisionContext.CurrentSquadUnit != null;
	}

	private static bool ConsiderNextSquadUnitExcept(Blackboard b, BaseUnitEntity exceptedUnit)
	{
		DecisionContext decisionContext = b.DecisionContext;
		decisionContext.ConsiderNextSquadUnit();
		if (decisionContext.CurrentSquadUnit == exceptedUnit)
		{
			decisionContext.ConsiderNextSquadUnit();
		}
		return decisionContext.CurrentSquadUnit != null;
	}

	private static void StoreMoveCommandForCurrentSquadUnit(Blackboard b)
	{
		DecisionContext decisionContext = b.DecisionContext;
		if (decisionContext.MoveCommand != null)
		{
			decisionContext.SquadUnitsMoveCommands.Add((decisionContext.CurrentSquadUnit, decisionContext.MoveCommand));
			decisionContext.MoveCommand = null;
		}
	}

	private static void SpendAllMovePointsOfCurrentSquadUnit(Blackboard b)
	{
		b.DecisionContext.CurrentSquadUnit.CombatState.SpendActionPointsAll(yellow: false, blue: true);
	}
}
