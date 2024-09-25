using System.Collections.Generic;
using Kingmaker.View;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public static class WarhammerPathHelper
{
	public static ForcedPath ConstructPathTo(Vector3 to, Dictionary<GraphNode, WarhammerPathPlayerCell> pathData)
	{
		return ConstructPathTo(ObstacleAnalyzer.GetNearestNode(to).node, pathData);
	}

	public static ForcedPath ConstructPathTo(GraphNode node, Dictionary<GraphNode, WarhammerPathPlayerCell> pathData)
	{
		List<GraphNode> list = new List<GraphNode>();
		List<Vector3> list2 = new List<Vector3>();
		GraphNode graphNode = node;
		while (graphNode != null && pathData.ContainsKey(graphNode))
		{
			list.Add(graphNode);
			list2.Add((Vector3)graphNode.position);
			graphNode = pathData[graphNode].ParentNode;
		}
		list.Reverse();
		list2.Reverse();
		return ForcedPath.Construct(list2, list);
	}

	public static ForcedPath ConstructPathTo(Vector3 to, Dictionary<GraphNode, WarhammerPathAiCell> pathData)
	{
		return ConstructPathTo(ObstacleAnalyzer.GetNearestNode(to).node, pathData);
	}

	public static ForcedPath ConstructPathTo(GraphNode node, Dictionary<GraphNode, WarhammerPathAiCell> pathData)
	{
		List<GraphNode> list = new List<GraphNode>();
		List<Vector3> list2 = new List<Vector3>();
		GraphNode graphNode = node;
		while (graphNode != null && pathData.ContainsKey(graphNode))
		{
			list.Add(graphNode);
			list2.Add((Vector3)graphNode.position);
			graphNode = pathData[graphNode].ParentNode;
		}
		list.Reverse();
		list2.Reverse();
		return ForcedPath.Construct(list2, list);
	}
}
