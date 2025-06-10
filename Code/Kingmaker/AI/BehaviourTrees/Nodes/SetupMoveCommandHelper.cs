using System.Collections.Generic;
using System.Linq;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public static class SetupMoveCommandHelper
{
	private struct PathData
	{
		public ForcedPath Path;

		public RuleCalculateMovementCost Cost;

		public int ThreatFactor;
	}

	public static bool CreatePathToBetterPlace(DecisionContext context, SetupMoveCommandMode mode, out ForcedPath path)
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
		path.Claim(context);
		if (path.path.Count < 2)
		{
			path.Release(context);
			AILogger.Instance.Log(new AILogReason(AILogReasonType.FoundPathTooShort));
			return false;
		}
		return true;
	}

	public static bool CreatePathToHoldPosition(DecisionContext context, SetupMoveCommandMode mode, out ForcedPath path)
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
				forcedPath.Claim(context);
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
			return (float)(((!CanStopAtNode(context, node, mode)) ? 1000 : 0) + p.ThreatFactor * 100) + p.Cost.ResultFullPathAPCost;
		});
		foreach (PathData item in list)
		{
			if (item.Path != pathData.Path)
			{
				item.Path?.Release(context);
			}
		}
		path = pathData.Path;
		return true;
	}

	public static bool CreatePathToUnit(DecisionContext context, SetupMoveCommandMode mode, out ForcedPath path)
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
			foreach (CustomGridNodeBase closeToUnitNode in GetCloseToUnitNodes(context, mode))
			{
				if (context.UnitMoveVariants.cells.TryGetValue(closeToUnitNode, out var value) && CanStopAtNode(context, value.Node, mode))
				{
					ForcedPath forcedPath = WarhammerPathHelper.ConstructPathTo(closeToUnitNode, context.UnitMoveVariants.cells);
					forcedPath.Claim(context);
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
				var (customGridNodeBase, fromSize) = GetTargetUnitNodeAndSize(context, mode);
				if (customGridNodeBase == null)
				{
					return false;
				}
				GraphNode graphNode = null;
				int num = int.MaxValue;
				foreach (KeyValuePair<GraphNode, WarhammerPathAiCell> item in context.UnitMoveVariants.cells.Where((KeyValuePair<GraphNode, WarhammerPathAiCell> c) => CanStopAtNode(context, c.Key, mode)))
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
				forcedPath2.Claim(context);
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
					item2.Path?.Release(context);
				}
			}
		}
	}

	private static HashSet<CustomGridNodeBase> GetCloseToUnitNodes(DecisionContext context, SetupMoveCommandMode mode)
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

	private static HashSet<CustomGridNodeBase> GetUnitNeighbourNodes(DecisionContext context, SetupMoveCommandMode mode)
	{
		switch (mode)
		{
		case SetupMoveCommandMode.LureCaster:
			return GetNodesNextToUnit(context.LuredTo).ToHashSet();
		case SetupMoveCommandMode.SquadLeader:
			return GetNodesNextToUnit(context.SquadLeaderNode, context.SquadLeader.SizeRect).ToHashSet();
		case SetupMoveCommandMode.SquadLeaderTarget:
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

	private static IEnumerable<CustomGridNodeBase> GetNodesNextToUnit(MechanicEntity unit)
	{
		return GetNodesNextToUnit(unit.GetNearestNodeXZ(), unit.SizeRect);
	}

	private static IEnumerable<CustomGridNodeBase> GetNodesNextToUnit(CustomGridNodeBase node, IntRect sizeRect)
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

	private static MechanicEntity GetClosestEnemy(DecisionContext context)
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

	public static bool CanStopAtNode(DecisionContext context, GraphNode node, SetupMoveCommandMode mode)
	{
		BaseUnitEntity unit = context.Unit;
		CustomGridNodeBase unitNode = context.UnitNode;
		if (mode != SetupMoveCommandMode.LureCaster && unit.Brain.IsHoldingPosition && context.HoldPositionNodes.Contains(unitNode) && !context.HoldPositionNodes.Contains(node))
		{
			return false;
		}
		NodeList nodes = GridAreaHelper.GetNodes(node, unit.SizeRect);
		foreach (var squadUnitsMoveCommand in context.SquadUnitsMoveCommands)
		{
			GraphNode graphNode = squadUnitsMoveCommand.cmd.ForcedPath?.path.Last();
			if (graphNode != null)
			{
				NodeList squadUnitNodes = GridAreaHelper.GetNodes(graphNode, squadUnitsMoveCommand.unit.SizeRect);
				if (nodes.Any((CustomGridNodeBase n) => squadUnitNodes.Contains(n)))
				{
					return false;
				}
			}
		}
		return context.UnitMoveVariants.cells[node].IsCanStand;
	}

	private static (CustomGridNodeBase, IntRect) GetTargetUnitNodeAndSize(DecisionContext context, SetupMoveCommandMode mode)
	{
		return mode switch
		{
			SetupMoveCommandMode.LureCaster => GetUnitNodeAndSize(context.LuredTo), 
			SetupMoveCommandMode.BetterPosition => GetUnitNodeAndSize(GetClosestEnemy(context)), 
			SetupMoveCommandMode.SquadLeader => (context.SquadLeaderNode, context.SquadLeader.SizeRect), 
			SetupMoveCommandMode.ClosestEnemy => GetUnitNodeAndSize(GetClosestEnemy(context)), 
			_ => (null, default(IntRect)), 
		};
	}

	private static (CustomGridNodeBase, IntRect) GetUnitNodeAndSize(MechanicEntity unit)
	{
		return (unit?.GetNearestNodeXZ(), unit?.SizeRect ?? default(IntRect));
	}
}
