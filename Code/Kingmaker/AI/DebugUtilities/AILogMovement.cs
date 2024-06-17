using Kingmaker.AI.BehaviourTrees.Nodes;
using Pathfinding;

namespace Kingmaker.AI.DebugUtilities;

public class AILogMovement : AILogObject
{
	private readonly bool isIntention;

	private readonly TaskNodeSetupMoveCommand.Mode moveTarget;

	private readonly GraphNode node;

	public static AILogMovement Intent(TaskNodeSetupMoveCommand.Mode moveTarget)
	{
		return new AILogMovement(isIntention: true, moveTarget, null);
	}

	public static AILogMovement Move(GraphNode node)
	{
		return new AILogMovement(isIntention: false, TaskNodeSetupMoveCommand.Mode.BetterPosition, node);
	}

	private AILogMovement(bool isIntention, TaskNodeSetupMoveCommand.Mode moveTarget, GraphNode node)
	{
		this.isIntention = isIntention;
		this.moveTarget = moveTarget;
		this.node = node;
	}

	public override string GetLogString()
	{
		if (!isIntention)
		{
			return $"Move to {node}";
		}
		return $"Try move to {moveTarget}";
	}
}
