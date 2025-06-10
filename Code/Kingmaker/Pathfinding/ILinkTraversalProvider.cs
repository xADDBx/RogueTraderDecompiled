using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Pathfinding;

namespace Kingmaker.Pathfinding;

public interface ILinkTraversalProvider
{
	IntRect SizeRect { get; }

	MechanicEntity Traverser { get; }

	bool AllowOtherToUseLink { get; }

	bool CanBuildPathThroughLink(GraphNode from, GraphNode to, [CanBeNull] INodeLink link);
}
