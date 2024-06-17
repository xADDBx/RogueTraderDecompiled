using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Kingmaker.AI.AreaScanning;
using Kingmaker.AI.AreaScanning.Scoring;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.AI.Scenarios;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Pathfinding;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class TaskNodeFindBetterPlace : TaskNode
{
	private TileScorer tileScorer;

	public TaskNodeFindBetterPlace(TileScorer tileScorer)
	{
		this.tileScorer = tileScorer;
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		DecisionContext decisionContext = blackboard.DecisionContext;
		decisionContext.IsMoveCommand = true;
		AiAreaScanner.PathData unitMoveVariants = decisionContext.UnitMoveVariants;
		GraphNode node = AstarPath.active.GetNearest(decisionContext.Unit.Position).node;
		AILogger.Instance.Log(AILogNode.Start(this));
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		List<CustomGridNodeBase> list = GetEnableToEndTurnNodes(decisionContext).EmptyIfNull().ToList();
		if (list.Empty())
		{
			AILogger.Instance.Log(new AILogReason(AILogReasonType.AcceptableNodesNotFound));
			decisionContext.IsMoveCommand = false;
			return Status.Failure;
		}
		if (!unitMoveVariants.cells.TryGetValue(list.First(), out var _))
		{
			AILogger.Instance.Log(new AILogReason(AILogReasonType.UnreachableNode, list.First()));
			decisionContext.IsMoveCommand = false;
			return Status.Failure;
		}
		ScoreOrder scoreOrder = new ScoreOrder(decisionContext.ScoreOrder);
		scoreOrder.SetFactor(ScoreType.ClosinessScore, ScoreFactor.Ignored);
		GraphNode highestScoreNode = tileScorer.GetHighestScoreNode(decisionContext, list, scoreOrder);
		bool num = highestScoreNode != node && highestScoreNode != null;
		stopwatch.Stop();
		AILogger.Instance.Log(new AILogElapsed(stopwatch.ElapsedMilliseconds));
		if (num)
		{
			WarhammerPathAiCell bestCell = unitMoveVariants.cells[highestScoreNode];
			while (bestCell.Length > decisionContext.Unit.CombatState.ActionPointsBlue)
			{
				AILogger.Instance.Log(AILogPositionSearch.FoundButTrim(AILogPositionSearch.PositionType.Better, bestCell.Node));
				bestCell = unitMoveVariants.cells[bestCell.ParentNode];
			}
			decisionContext.FoundBetterPlace = new DecisionContext.BetterPlace
			{
				PathData = unitMoveVariants,
				BestCell = bestCell
			};
			AILogger.Instance.Log(AILogPositionSearch.Found(AILogPositionSearch.PositionType.Better, bestCell.Node));
			decisionContext.IsMoveCommand = false;
			return Status.Success;
		}
		if (highestScoreNode == node)
		{
			decisionContext.FoundBetterPlace = new DecisionContext.BetterPlace
			{
				PathData = unitMoveVariants,
				BestCell = unitMoveVariants.startCell
			};
			AILogger.Instance.Log(new AILogReason(AILogReasonType.AlreadyOnBestPosition, unitMoveVariants.startCell.Node));
			decisionContext.IsMoveCommand = false;
			return Status.Success;
		}
		AILogger.Instance.Log(new AILogReason(AILogReasonType.BetterPositionNotFound));
		decisionContext.IsMoveCommand = false;
		return Status.Failure;
	}

	private IEnumerable<CustomGridNodeBase> GetEnableToEndTurnNodes(DecisionContext context)
	{
		AiAreaScanner.PathData pathData = context.UnitMoveVariants;
		PartUnitBrain brain = context.Unit.Brain;
		if (pathData.IsZero)
		{
			yield break;
		}
		IEnumerable<CustomGridNodeBase> enumerable;
		if (brain.IsHoldingPosition)
		{
			HoldPositionScenario holdPositionScenario = (HoldPositionScenario)brain.CurrentScenario;
			List<GraphNode> holdPositionNodes = context.HoldPositionNodes;
			enumerable = from k in pathData.cells.Keys
				where holdPositionNodes.Contains(k) && pathData.cells[k].IsCanStand
				select (CustomGridNodeBase)k;
			if (enumerable.Empty())
			{
				float num = float.MaxValue;
				GraphNode graphNode = null;
				GraphNode node = AstarPath.active.GetNearest(holdPositionScenario.Target.Point).node;
				foreach (var (graphNode3, _) in pathData.cells)
				{
					if (CanStopAtNode(context, graphNode3))
					{
						int num2 = WarhammerGeometryUtils.DistanceToInCells(graphNode3.Vector3Position, context.Unit.SizeRect, node.Vector3Position, default(IntRect));
						if ((float)num2 < num)
						{
							num = num2;
							graphNode = graphNode3;
						}
					}
				}
				if (graphNode != null)
				{
					yield return (CustomGridNodeBase)graphNode;
				}
				yield break;
			}
		}
		else
		{
			enumerable = pathData.cells.Keys.Select((GraphNode k) => (CustomGridNodeBase)k);
		}
		foreach (CustomGridNodeBase item in enumerable)
		{
			if (pathData.cells.TryGetValue(item, out var _) && CanStopAtNode(context, item))
			{
				yield return item;
			}
		}
	}

	private bool CanStopAtNode(DecisionContext context, GraphNode node)
	{
		if (context.SquadUnitsMoveCommands.Any(((BaseUnitEntity unit, UnitMoveToProperParams cmd) x) => x.cmd.ForcedPath.path.Last() == node))
		{
			return false;
		}
		BaseUnitEntity unit = context.Unit;
		if (unit.GetSquadOptional() != null && unit != context.SquadLeader && !InLeaderRange(context, node, 2))
		{
			return false;
		}
		return context.UnitMoveVariants.cells[node].IsCanStand;
	}

	private bool InLeaderRange(DecisionContext context, GraphNode node, int radius)
	{
		return WarhammerGeometryUtils.DistanceToInCells(context.SquadLeaderNode.Vector3Position, context.SquadLeader.SizeRect, node.Vector3Position, default(IntRect)) <= radius;
	}
}
