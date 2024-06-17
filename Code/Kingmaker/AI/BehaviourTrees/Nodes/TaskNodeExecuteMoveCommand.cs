using System.Collections.Generic;
using System.Linq;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class TaskNodeExecuteMoveCommand : CoroutineTaskNode
{
	protected override IEnumerator<Status> CreateCoroutine(Blackboard blackboard)
	{
		DecisionContext context = blackboard.DecisionContext;
		BaseUnitEntity unit = context.Unit;
		UnitMoveToProperParams moveCommand = ((context.SquadPhase == SquadPhase.Move) ? (from p in context.SquadUnitsMoveCommands
			where p.unit == unit
			select p.cmd).FirstOrDefault() : context.MoveCommand);
		if (moveCommand == null)
		{
			AILogger.Instance.Log(new AILogReason(AILogReasonType.MoveCommandNotSet));
			yield return Status.Success;
			yield break;
		}
		while (!unit.Brain.IsActingEnabled)
		{
			yield return Status.Running;
		}
		AILogger.Instance.Log(AILogMovement.Move(moveCommand.ForcedPath.path.Last()));
		unit.Commands.Run(moveCommand);
		unit.Brain.IsIdling = false;
		if (context.SquadPhase == SquadPhase.Move)
		{
			context.SquadUnitsMoveCommands.Remove((unit, moveCommand));
		}
		else
		{
			context.MoveCommand = null;
		}
		yield return Status.Success;
	}
}
