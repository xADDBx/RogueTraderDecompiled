using Pathfinding;

namespace Kingmaker.AI.DebugUtilities;

public class AILogPositionSearch : AILogObject
{
	public enum PositionType
	{
		Better,
		Retreat
	}

	private readonly PositionType type;

	private readonly GraphNode node;

	private readonly bool shouldTrim;

	public static AILogPositionSearch Found(PositionType type, GraphNode node)
	{
		return new AILogPositionSearch(type, node);
	}

	public static AILogPositionSearch FoundButTrim(PositionType type, GraphNode node)
	{
		return new AILogPositionSearch(type, node);
	}

	private AILogPositionSearch(PositionType type, GraphNode node)
	{
		this.type = type;
		this.node = node;
	}

	public override string GetLogString()
	{
		if (!shouldTrim)
		{
			return $"Found {type} position: {node}";
		}
		return $"Found {type} position {node} is too far, trim";
	}
}
