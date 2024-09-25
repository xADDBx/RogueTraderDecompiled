using Pathfinding;

namespace Kingmaker.Pathfinding;

public interface IWarhammerTraversalProvider : ITraversalProvider
{
	bool CanTraverseEndNode(GraphNode node, int direction);

	bool CanTraverseAlongDirection(Path path, GraphNode node, int direction);
}
