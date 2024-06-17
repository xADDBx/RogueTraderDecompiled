using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public static class NodeLinksExtensions
{
	public static bool AreConnected(GraphNode from, GraphNode to, out INodeLink currentLink)
	{
		currentLink = null;
		if (!(from is CustomGridNodeBase customGridNodeBase) || !(to is CustomGridNodeBase customGridNodeBase2))
		{
			return false;
		}
		if (customGridNodeBase.connections == null)
		{
			return false;
		}
		for (int i = 0; i < customGridNodeBase.connections.Length; i++)
		{
			CustomConnection customConnection = customGridNodeBase.connections[i];
			if (customConnection.Node == customGridNodeBase2)
			{
				currentLink = customConnection.Link;
				return true;
			}
		}
		return false;
	}

	public static bool AreConnected(Vector3 from, Vector2 to, out GraphNode fromNode, out GraphNode toNode, out INodeLink currentLink)
	{
		fromNode = null;
		toNode = null;
		currentLink = null;
		CustomGridNodeBase customGridNodeBase = (CustomGridNodeBase)ObstacleAnalyzer.GetNearestNode(from).node;
		if (customGridNodeBase.connections == null)
		{
			return false;
		}
		for (int i = 0; i < customGridNodeBase.connections.Length; i++)
		{
			CustomConnection customConnection = customGridNodeBase.connections[i];
			if (to == customConnection.Node.Vector3Position.To2D())
			{
				currentLink = customConnection.Link;
				toNode = customConnection.Node;
				fromNode = customGridNodeBase;
				return true;
			}
		}
		return false;
	}

	public static bool IsNodeLinked(this GraphNode node)
	{
		if (node is CustomGridNodeBase { connections: var connections })
		{
			if (connections != null)
			{
				return connections.Length > 0;
			}
			return false;
		}
		return false;
	}
}
