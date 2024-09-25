using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Squads;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class TaskNodeWaitCommandsDone : TaskNode
{
	protected override Status TickInternal(Blackboard blackboard)
	{
		if (Game.Instance.AiBrainController.IsBusy)
		{
			return Status.Running;
		}
		if (!GetActingUnits(blackboard).ToList().All((BaseUnitEntity u) => u.Commands.Empty && u.State.CanActInTurnBased))
		{
			return Status.Running;
		}
		return Status.Success;
	}

	private IEnumerable<BaseUnitEntity> GetActingUnits(Blackboard blackboard)
	{
		if (blackboard.DecisionContext.SquadPhase != SquadPhase.Move)
		{
			return TempList.Get<BaseUnitEntity>().Append(blackboard.Unit);
		}
		return ((UnitSquad)Game.Instance.TurnController.CurrentUnit).GetConsciousUnits();
	}
}
