using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.View.Mechanics;
using Kingmaker.View.Mechanics.Entities;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View.MapObjects.SriptZones;

public static class EditorGridHelper
{
	public static IEnumerable<Vector3> GetPointsInsideScriptZone(IScriptZoneShape shape, Vector3 point)
	{
		AstarPath.FindAstarPath();
		if (!AstarPath.active || !(AstarPath.active.data.graphs[0] is CustomGridGraph customGridGraph))
		{
			return null;
		}
		Int3 centerNode = ConvertPointToGridCoordinates(point, customGridGraph);
		if (customGridGraph.nodes != null)
		{
			return FindPointsInsideScriptZoneNavmesh(shape, customGridGraph);
		}
		return FindPointsInsideScriptZoneSimple(shape, centerNode, customGridGraph);
	}

	private static IEnumerable<Vector3> FindPointsInsideScriptZoneNavmesh(IScriptZoneShape shape, CustomGridGraph graph)
	{
		List<GraphNode> nodesInRegion = graph.GetNodesInRegion(shape.GetBounds());
		foreach (GraphNode item in nodesInRegion)
		{
			if (shape.Contains(item.Vector3Position))
			{
				yield return item.Vector3Position;
			}
		}
	}

	private static IEnumerable<Vector3> FindPointsInsideScriptZoneSimple(IScriptZoneShape shape, Int3 centerNode, CustomGridGraph graph)
	{
		HashSet<Int3> searchList = new HashSet<Int3> { centerNode };
		HashSet<Int3> doneNodes = new HashSet<Int3>();
		float[,] neighborStepDirection = new float[4, 2]
		{
			{ 0f, graph.nodeSize },
			{
				0f,
				0f - graph.nodeSize
			},
			{ graph.nodeSize, 0f },
			{
				0f - graph.nodeSize,
				0f
			}
		};
		while (searchList.Count > 0)
		{
			Int3 currentNode = searchList.FirstOrDefault();
			Vector3 currentNodePosition = FindWorldPositionOfNode(currentNode, graph, shape.Center().y);
			if (!shape.Contains(currentNodePosition))
			{
				searchList.Remove(currentNode);
				continue;
			}
			yield return currentNodePosition;
			for (int i = 0; i < 4; i++)
			{
				Int3 item = ConvertPointToGridCoordinates(new Vector3(currentNodePosition.x + neighborStepDirection[i, 0], currentNodePosition.y, currentNodePosition.z + neighborStepDirection[i, 1]), graph);
				if (!doneNodes.Contains(item) && !searchList.Contains(item))
				{
					searchList.Add(item);
				}
			}
			doneNodes.Add(currentNode);
			searchList.Remove(currentNode);
		}
	}

	private static Int3 ConvertPointToGridCoordinates(Vector3 point, CustomGridGraph graph)
	{
		int value = Mathf.FloorToInt((point.x - graph.size.x / 2f) / 1.Cells().Meters);
		return new Int3(_z: Math.Abs(Mathf.FloorToInt((point.z - graph.size.y / 2f) / 1.Cells().Meters)), _x: Math.Abs(value), _y: 0);
	}

	private static Vector3 FindWorldPositionOfNode(Int3 graphPosition, CustomGridGraph graph, float height)
	{
		float num = graph.size.x / 2f - graph.center.x;
		float num2 = graph.size.y / 2f - graph.center.z;
		float x = num + 1.Cells().Meters / 2f - 1.Cells().Meters * (float)graphPosition.x;
		float z = num2 + 1.Cells().Meters / 2f - 1.Cells().Meters * (float)graphPosition.z;
		return new Vector3(x, height, z);
	}

	public static IEnumerable<Vector3> GetCellsCoveredByMechanicEntity([CanBeNull] MechanicEntity data, MechanicEntityView view)
	{
		if (data == null)
		{
			DestructibleEntityView destructibleEntityView = view as DestructibleEntityView;
			AreaEffectView areaEffectView = view as AreaEffectView;
			if (!destructibleEntityView && !areaEffectView)
			{
				return null;
			}
			AstarPath.FindAstarPath();
			if (!AstarPath.active || !(AstarPath.active.data.graphs[0] is CustomGridGraph graph))
			{
				return null;
			}
			Vector3 vector = Vector3.zero;
			Vector2 vector2 = Vector2.zero;
			if ((bool)destructibleEntityView)
			{
				vector = GetStarterNode(destructibleEntityView.Bounds.min, destructibleEntityView.ViewTransform.position.y, graph);
				vector2 = destructibleEntityView.Bounds.size;
			}
			else if ((bool)areaEffectView)
			{
				Vector3 vector3 = areaEffectView.Shape.Center();
				vector = GetStarterNode(new Vector3(vector3.x, vector3.z), areaEffectView.ViewTransform.position.y, graph);
				vector2 = Vector3.one;
			}
			int num = Math.Max(0, Mathf.RoundToInt(vector2.x / GraphParamsMechanicsCache.GridCellSize) - 1);
			int num2 = Math.Max(0, Mathf.RoundToInt(vector2.y / GraphParamsMechanicsCache.GridCellSize) - 1);
			List<Vector3> list = new List<Vector3>();
			for (int j = 0; j < num + 1; j++)
			{
				for (int k = 0; k < num2 + 1; k++)
				{
					list.Add(new Vector3((float)j * 1.Cells().Meters + vector.x, vector.y, (float)k * 1.Cells().Meters + vector.z));
				}
			}
			return list;
		}
		return from i in data.GetOccupiedNodes()
			select i.Vector3Position;
	}

	private static Vector3 GetStarterNode(Vector3 center, float yPos, CustomGridGraph graph)
	{
		return FindWorldPositionOfNode(ConvertPointToGridCoordinates(new Vector3(center.x, 0f, center.y), graph), graph, yPos);
	}
}
