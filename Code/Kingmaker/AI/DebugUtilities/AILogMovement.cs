using Kingmaker.AI.BehaviourTrees.Nodes;
using Pathfinding;

namespace Kingmaker.AI.DebugUtilities;

public class AILogMovement : AILogObject
{
	private readonly bool isIntention;

	private readonly SetupMoveCommandMode moveTarget;

	private readonly GraphNode node;

	public static AILogMovement Intent(SetupMoveCommandMode moveTarget)
	{
		return new AILogMovement(isIntention: true, moveTarget, null);
	}

	public static AILogMovement Move(GraphNode node)
	{
		return new AILogMovement(isIntention: false, SetupMoveCommandMode.BetterPosition, node);
	}

	private AILogMovement(bool isIntention, SetupMoveCommandMode moveTarget, GraphNode node)
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
