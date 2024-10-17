using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Pathfinding;

namespace Kingmaker.Visual.Critters;

public class RabbitLinkTraversalProvider : ILinkTraversalProvider
{
	IntRect ILinkTraversalProvider.SizeRect => default(IntRect);

	MechanicEntity ILinkTraversalProvider.Traverser => null;

	bool ILinkTraversalProvider.AllowOtherToUseLink => false;

	bool ILinkTraversalProvider.CanBuildPathThroughLink(GraphNode from, GraphNode to, INodeLink link)
	{
		return false;
	}
}
