using System.Collections.Generic;
using System.Linq;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class TaskNodeSetupMoveCommand : TaskNode
{
	private struct PathData
	{
		public ForcedPath Path;

		public RuleCalculateMovementCost Cost;

		public int ThreatFactor;
	}

	public enum Mode
	{
		BetterPosition,
		ClosestEnemy,
		LureCaster,
		SquadLeader,
		SquadLeaderTarget,
		HoldPosition
	}

	private Mode m_Mode;

	public static TaskNodeSetupMoveCommand ToBetterPosition()
	{
		return new TaskNodeSetupMoveCommand(Mode.BetterPosition);
	}

	public static TaskNodeSetupMoveCommand ToClosestEnemy()
	{
		return new TaskNodeSetupMoveCommand(Mode.ClosestEnemy);
	}

	public static TaskNodeSetupMoveCommand ToLureCaster()
	{
		return new TaskNodeSetupMoveCommand(Mode.LureCaster);
	}

	public static TaskNodeSetupMoveCommand ToSquadLeader()
	{
		return new TaskNodeSetupMoveCommand(Mode.SquadLeader);
	}

	public static TaskNodeSetupMoveCommand ToSquadLeaderTarget()
	{
		return new TaskNodeSetupMoveCommand(Mode.SquadLeaderTarget);
	}

	public static TaskNodeSetupMoveCommand ToHoldPosition()
	{
		return new TaskNodeSetupMoveCommand(Mode.HoldPosition);
	}

	private TaskNodeSetupMoveCommand(Mode mode)
	{
		m_Mode = mode;
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		AILogger.Instance.Log(AILogMovement.Intent(m_Mode));
		DecisionContext decisionContext = blackboard.DecisionContext;
		decisionContext.IsMoveCommand = true;
		if (!CreatePath(decisionContext, out var path))
		{
			path?.Release(this);
			return Status.Failure;
		}
		if (path == null)
		{
			return Status.Success;
		}
		RuleCalculateMovementCost ruleCalculateMovementCost = Rulebook.Trigger(new RuleCalculateMovementCost(decisionContext.Unit, path));
		int num = ruleCalculateMovementCost.ResultPointCount;
		while (num > 0)
		{
			GraphNode graphNode = path.path[num - 1];
			if (CanStopAtNode(decisionContext, graphNode))
			{
				break;
			}
			num--;
			AILogger.Instance.Log(new AILogReason(AILogReasonType.UnreachableNodeTrimPath, graphNode));
		}
		if (num < 2)
		{
			path.Release(this);
			decisionContext.IsMoveCommand = false;
			return Status.Failure;
		}
		float[] resultAPCostPerPoint = ruleCalculateMovementCost.ResultAPCostPerPoint;
		ForcedPath path2 = ForcedPath.Construct(path.vectorPath.Take(num), path.path.Take(num));
		path.Release(this);
		BaseUnitEntity unit = decisionContext.Unit;
		UnitMoveToProperParams moveCommand = new UnitMoveToProperParams(path2, unit.Blueprint.WarhammerMovementApPerCell, resultAPCostPerPoint);
		decisionContext.MoveCommand = moveCommand;
		decisionContext.IsMoveCommand = false;
		return Status.Success;
	}

	private bool CreatePath(DecisionContext context, out ForcedPath path)
	{
		if (m_Mode == Mode.BetterPosition)
		{
			return CreatePathToBetterPlace(context, out path);
		}
		if (m_Mode == Mode.HoldPosition)
		{
			return CreatePathToHoldPosition(context, out path);
		}
		return CreatePathToUnit(context, out path);
	}

	private bool CreatePathToBetterPlace(DecisionContext context, out ForcedPath path)
	{
		path = null;
		if (context.FoundBetterPlace.PathData.IsZero || context.UnitMoveVariants.IsZero)
		{
			return false;
		}
		if ((CustomGridNodeBase)AstarPath.active.GetNearest(context.Unit.Position).node == context.FoundBetterPlace.BestCell.Node)
		{
			AILogger.Instance.Log(new AILogReason(AILogReasonType.NoNeedToMove));
			return true;
		}
		path = WarhammerPathHelper.ConstructPathTo(context.FoundBetterPlace.BestCell.Node, context.UnitMoveVariants.cells);
		path.Claim(this);
		if (path.path.Count < 2)
		{
			path.Release(this);
			AILogger.Instance.Log(new AILogReason(AILogReasonType.FoundPathTooShort));
			return false;
		}
		return true;
	}

	private bool CreatePathToHoldPosition(DecisionContext context, out ForcedPath path)
	{
		path = null;
		if (context.HoldPositionNodes.Count == 0)
		{
			return false;
		}
		if (context.UnitMoveVariants.IsZero)
		{
			return false;
		}
		BaseUnitEntity unit = context.Unit;
		List<PathData> list = new List<PathData>();
		foreach (GraphNode holdPositionNode in context.HoldPositionNodes)
		{
			if (context.UnitMoveVariants.cells.TryGetValue(holdPositionNode, out var value))
			{
				ForcedPath forcedPath = WarhammerPathHelper.ConstructPathTo(holdPositionNode, context.UnitMoveVariants.cells);
				forcedPath.Claim(this);
				RuleCalculateMovementCost cost = Rulebook.Trigger(new RuleCalculateMovementCost(unit, forcedPath, calcFullPathApCost: true));
				int threatFactor = ((!unit.Brain.IsUsualMeleeUnit) ? (value.EnteredAoE + value.StepsInsideDamagingAoE + value.ProvokedAttacks + ((unit.Brain.ResponseToAoOThreat && unit.IsEngagedInPosition(holdPositionNode.Vector3Position)) ? 1 : 0)) : 0);
				list.Add(new PathData
				{
					Path = forcedPath,
					Cost = cost,
					ThreatFactor = threatFactor
				});
			}
		}
		PathData pathData = list.MinBy(delegate(PathData p)
		{
			GraphNode node = p.Path.path.Last();
			return (float)(((!CanStopAtNode(context, node)) ? 1000 : 0) + p.ThreatFactor * 100) + p.Cost.ResultFullPathAPCost;
		});
		foreach (PathData item in list)
		{
			if (item.Path != pathData.Path)
			{
				item.Path?.Release(this);
			}
		}
		path = pathData.Path;
		return true;
	}

	private bool CreatePathToUnit(DecisionContext context, out ForcedPath path)
	{
		List<PathData> list = new List<PathData>();
		path = null;
		try
		{
			BaseUnitEntity unit = context.Unit;
			if (context.UnitMoveVariants.IsZero)
			{
				return false;
			}
			foreach (CustomGridNodeBase closeToUnitNode in GetCloseToUnitNodes(context, m_Mode))
			{
				if (context.UnitMoveVariants.cells.TryGetValue(closeToUnitNode, out var value) && CanStopAtNode(context, value.Node))
				{
					ForcedPath forcedPath = WarhammerPathHelper.ConstructPathTo(closeToUnitNode, context.UnitMoveVariants.cells);
					forcedPath.Claim(this);
					RuleCalculateMovementCost cost = Rulebook.Trigger(new RuleCalculateMovementCost(unit, forcedPath, calcFullPathApCost: true));
					int threatFactor = ((!unit.Brain.IsUsualMeleeUnit) ? (value.EnteredAoE + value.StepsInsideDamagingAoE + value.ProvokedAttacks) : 0);
					list.Add(new PathData
					{
						Path = forcedPath,
						Cost = cost,
						ThreatFactor = threatFactor
					});
				}
			}
			PathData pathData = list.MinBy((PathData p) => (float)(p.ThreatFactor * 100) + p.Cost.ResultFullPathAPCost);
			if (pathData.Path == null || pathData.Cost == null)
			{
				var (customGridNodeBase, fromSize) = GetTargetUnitNodeAndSize(context, m_Mode);
				if (customGridNodeBase == null)
				{
					return false;
				}
				GraphNode graphNode = null;
				int num = int.MaxValue;
				foreach (KeyValuePair<GraphNode, WarhammerPathAiCell> item in context.UnitMoveVariants.cells.Where((KeyValuePair<GraphNode, WarhammerPathAiCell> c) => CanStopAtNode(context, c.Key)))
				{
					int num2 = WarhammerGeometryUtils.DistanceToInCells(customGridNodeBase.Vector3Position, fromSize, item.Key.Vector3Position, unit.SizeRect);
					if (num2 < num)
					{
						num = num2;
						graphNode = item.Key;
					}
				}
				if (graphNode == null)
				{
					return false;
				}
				ForcedPath forcedPath2 = WarhammerPathHelper.ConstructPathTo(graphNode, context.UnitMoveVariants.cells);
				forcedPath2.Claim(this);
				RuleCalculateMovementCost cost2 = Rulebook.Trigger(new RuleCalculateMovementCost(unit, forcedPath2, calcFullPathApCost: true));
				PathData pathData2 = default(PathData);
				pathData2.Path = forcedPath2;
				pathData2.Cost = cost2;
				pathData = pathData2;
			}
			path = pathData.Path;
			return true;
		}
		finally
		{
			foreach (PathData item2 in list)
			{
				if (item2.Path != path)
				{
					item2.Path?.Release(this);
				}
			}
		}
	}

	private HashSet<CustomGridNodeBase> GetCloseToUnitNodes(DecisionContext context, Mode mode)
	{
		HashSet<CustomGridNodeBase> unitNeighbourNodes = GetUnitNeighbourNodes(context, mode);
		IntRect sizeRect = context.Unit.SizeRect;
		if (sizeRect.Width == 1 && sizeRect.Height == 1)
		{
			return unitNeighbourNodes;
		}
		if (unitNeighbourNodes.Count == 0)
		{
			return unitNeighbourNodes;
		}
		CustomGridGraph customGridGraph = unitNeighbourNodes.First().Graph as CustomGridGraph;
		foreach (CustomGridNodeBase item in unitNeighbourNodes.ToTempList())
		{
			for (int i = 0; i < sizeRect.Width; i++)
			{
				for (int j = 0; j < sizeRect.Height; j++)
				{
					unitNeighbourNodes.Add(customGridGraph.GetNode(item.XCoordinateInGrid - i, item.ZCoordinateInGrid - j));
				}
			}
		}
		return unitNeighbourNodes;
	}

	private HashSet<CustomGridNodeBase> GetUnitNeighbourNodes(DecisionContext context, Mode mode)
	{
		switch (mode)
		{
		case Mode.LureCaster:
			return GetNodesNextToUnit(context.LuredTo).ToHashSet();
		case Mode.SquadLeader:
			return GetNodesNextToUnit(context.SquadLeaderNode, context.SquadLeader.SizeRect).ToHashSet();
		case Mode.SquadLeaderTarget:
		{
			HashSet<CustomGridNodeBase> hashSet2 = GetNodesNextToUnit(context.SquadLeaderTarget).ToHashSet();
			hashSet2.IntersectWith(GetNodesNextToUnit(context.SquadLeaderNode, context.SquadLeader.SizeRect));
			return hashSet2;
		}
		default:
		{
			HashSet<CustomGridNodeBase> hashSet = new HashSet<CustomGridNodeBase>();
			{
				foreach (TargetInfo hatedTarget in context.HatedTargets)
				{
					hashSet.UnionWith(GetNodesNextToUnit(hatedTarget.Entity));
				}
				return hashSet;
			}
		}
		}
	}

	private IEnumerable<CustomGridNodeBase> GetNodesNextToUnit(MechanicEntity unit)
	{
		return GetNodesNextToUnit(unit.GetNearestNodeXZ(), unit.SizeRect);
	}

	private IEnumerable<CustomGridNodeBase> GetNodesNextToUnit(CustomGridNodeBase node, IntRect sizeRect)
	{
		List<CustomGridNodeBase> unitNodes = GridAreaHelper.GetOccupiedNodes(node, sizeRect).ToList();
		CustomGridGraph graph = unitNodes[0].Graph as CustomGridGraph;
		foreach (CustomGridNodeBase unitNode in unitNodes)
		{
			for (int x = unitNode.XCoordinateInGrid - 1; x <= unitNode.XCoordinateInGrid + 1; x++)
			{
				for (int z = unitNode.ZCoordinateInGrid - 1; z <= unitNode.ZCoordinateInGrid + 1; z++)
				{
					CustomGridNodeBase node2 = graph.GetNode(x, z);
					if (node2 != null && !unitNodes.Contains(node2) && unitNode.ContainsConnection(node2))
					{
						yield return node2;
					}
				}
			}
		}
	}

	private MechanicEntity GetClosestEnemy(DecisionContext context)
	{
		BaseUnitEntity unit = context.Unit;
		int num = int.MaxValue;
		MechanicEntity result = null;
		foreach (TargetInfo hatedTarget in context.HatedTargets)
		{
			int num2 = unit.DistanceToInCells(hatedTarget.Entity);
			if (num2 < num)
			{
				num = num2;
				result = hatedTarget.Entity;
			}
		}
		return result;
	}

	private bool CanStopAtNode(DecisionContext context, GraphNode node)
	{
		BaseUnitEntity unit = context.Unit;
		CustomGridNodeBase unitNode = context.UnitNode;
		if (m_Mode != Mode.LureCaster && unit.Brain.IsHoldingPosition && context.HoldPositionNodes.Contains(unitNode) && !context.HoldPositionNodes.Contains(node))
		{
			return false;
		}
		if (context.SquadUnitsMoveCommands.Any(((BaseUnitEntity unit, UnitMoveToProperParams cmd) x) => x.cmd.ForcedPath.path.Last() == node))
		{
			return false;
		}
		return context.UnitMoveVariants.cells[node].IsCanStand;
	}

	private (CustomGridNodeBase, IntRect) GetTargetUnitNodeAndSize(DecisionContext context, Mode mode)
	{
		return mode switch
		{
			Mode.LureCaster => GetUnitNodeAndSize(context.LuredTo), 
			Mode.BetterPosition => GetUnitNodeAndSize(GetClosestEnemy(context)), 
			Mode.SquadLeader => (context.SquadLeaderNode, context.SquadLeader.SizeRect), 
			_ => (null, default(IntRect)), 
		};
	}

	private (CustomGridNodeBase, IntRect) GetUnitNodeAndSize(MechanicEntity unit)
	{
		return (unit.GetNearestNodeXZ(), unit.SizeRect);
	}
}
