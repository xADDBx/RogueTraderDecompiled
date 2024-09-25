using System.Collections.Generic;
using Pathfinding;

namespace Kingmaker.Pathfinding;

public static class NodeLinksCache
{
	private static readonly Dictionary<GraphNode, INodeLink> Reference = new Dictionary<GraphNode, INodeLink>();

	public static bool AnyLinkActive => Reference.Count > 0;

	public static void Add(GraphNode node, INodeLink link)
	{
		if (!Reference.ContainsKey(node))
		{
			Reference.Add(node, link);
		}
		Reference[node] = link;
	}

	public static void Remove(GraphNode node)
	{
		Reference.Remove(node);
	}
}
