using Kingmaker.Pathfinding;

namespace Kingmaker.AI.DebugUtilities;

public class AILogTrajectorySearch : AILogObject
{
	private readonly ShipPath.DirectionalPathNode node;

	private readonly float score;

	public AILogTrajectorySearch(ShipPath.DirectionalPathNode node, float score)
	{
		this.node = node;
		this.score = score;
	}

	public override string GetLogString()
	{
		return $"Found best trajectory, final node is {node}, score is {score}";
	}
}
