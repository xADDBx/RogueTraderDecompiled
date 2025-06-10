using System.Linq;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Commands;
using Pathfinding;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class TaskNodeSetupMoveCommand : TaskNode
{
	private SetupMoveCommandMode m_Mode;

	public static TaskNodeSetupMoveCommand ToBetterPosition()
	{
		return new TaskNodeSetupMoveCommand(SetupMoveCommandMode.BetterPosition);
	}

	public static TaskNodeSetupMoveCommand ToClosestEnemy()
	{
		return new TaskNodeSetupMoveCommand(SetupMoveCommandMode.ClosestEnemy);
	}

	public static TaskNodeSetupMoveCommand ToLureCaster()
	{
		return new TaskNodeSetupMoveCommand(SetupMoveCommandMode.LureCaster);
	}

	public static TaskNodeSetupMoveCommand ToSquadLeader()
	{
		return new TaskNodeSetupMoveCommand(SetupMoveCommandMode.SquadLeader);
	}

	public static TaskNodeSetupMoveCommand ToSquadLeaderTarget()
	{
		return new TaskNodeSetupMoveCommand(SetupMoveCommandMode.SquadLeaderTarget);
	}

	public static TaskNodeSetupMoveCommand ToHoldPosition()
	{
		return new TaskNodeSetupMoveCommand(SetupMoveCommandMode.HoldPosition);
	}

	private TaskNodeSetupMoveCommand(SetupMoveCommandMode mode)
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
			if (SetupMoveCommandHelper.CanStopAtNode(decisionContext, graphNode, m_Mode))
			{
				break;
			}
			num--;
			AILogger.Instance.Log(new AILogReason(AILogReasonType.UnreachableNodeTrimPath, graphNode));
		}
		if (num < 2)
		{
			path.Release(decisionContext);
			decisionContext.IsMoveCommand = false;
			return Status.Failure;
		}
		float[] resultAPCostPerPoint = ruleCalculateMovementCost.ResultAPCostPerPoint;
		ForcedPath path2 = ForcedPath.Construct(path.vectorPath.Take(num), path.path.Take(num));
		path.Release(decisionContext);
		BaseUnitEntity unit = decisionContext.Unit;
		UnitMoveToProperParams moveCommand = new UnitMoveToProperParams(path2, unit.Blueprint.WarhammerMovementApPerCell, resultAPCostPerPoint);
		decisionContext.MoveCommand = moveCommand;
		decisionContext.IsMoveCommand = false;
		return Status.Success;
	}

	private bool CreatePath(DecisionContext context, out ForcedPath path)
	{
		if (m_Mode == SetupMoveCommandMode.BetterPosition)
		{
			return SetupMoveCommandHelper.CreatePathToBetterPlace(context, m_Mode, out path);
		}
		if (m_Mode == SetupMoveCommandMode.HoldPosition)
		{
			return SetupMoveCommandHelper.CreatePathToHoldPosition(context, m_Mode, out path);
		}
		return SetupMoveCommandHelper.CreatePathToUnit(context, m_Mode, out path);
	}
}
