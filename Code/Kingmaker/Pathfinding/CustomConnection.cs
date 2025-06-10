using JetBrains.Annotations;
using Pathfinding;

namespace Kingmaker.Pathfinding;

public struct CustomConnection
{
	public GraphNode Node;

	[CanBeNull]
	public INodeLink Link;

	public uint Cost;

	public byte ShapeEdge;

	public CustomConnection(GraphNode node, uint cost, byte shapeEdge = byte.MaxValue, INodeLink link = null)
	{
		Node = node;
		Cost = cost;
		ShapeEdge = shapeEdge;
		Link = link;
	}

	public override int GetHashCode()
	{
		return Node.GetHashCode() ^ (int)Cost;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		CustomConnection customConnection = (CustomConnection)obj;
		if (customConnection.Node == Node && customConnection.Cost == Cost && customConnection.ShapeEdge == ShapeEdge)
		{
			return customConnection.Link == Link;
		}
		return false;
	}
}
