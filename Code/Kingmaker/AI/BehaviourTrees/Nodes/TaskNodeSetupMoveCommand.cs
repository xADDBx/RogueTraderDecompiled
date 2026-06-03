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
	public SetupMoveCommandMode Mode { get; }

	public static TaskNodeSetupMoveCommand ToBetterPosition(string debugDescription = null)
	{
		return new TaskNodeSetupMoveCommand(SetupMoveCommandMode.BetterPosition, debugDescription);
	}

	public static TaskNodeSetupMoveCommand ToClosestEnemy(string debugDescription = null)
	{
		return new TaskNodeSetupMoveCommand(SetupMoveCommandMode.ClosestEnemy, debugDescription);
	}

	public static TaskNodeSetupMoveCommand ToLureCaster(string debugDescription = null)
	{
		return new TaskNodeSetupMoveCommand(SetupMoveCommandMode.LureCaster, debugDescription);
	}

	public static TaskNodeSetupMoveCommand ToSquadLeader(string debugDescription = null)
	{
		return new TaskNodeSetupMoveCommand(SetupMoveCommandMode.SquadLeader, debugDescription);
	}

	public static TaskNodeSetupMoveCommand ToSquadLeaderTarget(string debugDescription = null)
	{
		return new TaskNodeSetupMoveCommand(SetupMoveCommandMode.SquadLeaderTarget, debugDescription);
	}

	public static TaskNodeSetupMoveCommand ToHoldPosition(string debugDescription = null)
	{
		return new TaskNodeSetupMoveCommand(SetupMoveCommandMode.HoldPosition, debugDescription);
	}

	private TaskNodeSetupMoveCommand(SetupMoveCommandMode mode, string debugDescription = null)
		: base(debugDescription)
	{
		Mode = mode;
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		AILogger.Instance.Log(AILogMovement.Intent(Mode));
		DecisionContext decisionContext = blackboard.DecisionContext;
		decisionContext.IsMoveCommand = true;
		if (!CreatePath(decisionContext, out var path))
		{
			path?.Release(this);
			base.FailReason = "No path to target according mode";
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
			if (SetupMoveCommandHelper.CanStopAtNode(decisionContext, graphNode, Mode))
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
			base.FailReason = "Can't reach target point";
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
		if (Mode == SetupMoveCommandMode.BetterPosition)
		{
			return SetupMoveCommandHelper.CreatePathToBetterPlace(context, Mode, out path);
		}
		if (Mode == SetupMoveCommandMode.HoldPosition)
		{
			return SetupMoveCommandHelper.CreatePathToHoldPosition(context, Mode, out path);
		}
		return SetupMoveCommandHelper.CreatePathToUnit(context, Mode, out path);
	}
}
