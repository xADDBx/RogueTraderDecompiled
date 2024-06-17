using System;
using Kingmaker.Pathfinding;
using Unity.Mathematics;
using UnityEngine;

namespace Kingmaker.UI.SurfaceCombatHUD;

[Serializable]
public struct GridSettings
{
	public float cellSize;

	public float2 positionOffset;

	public int gridDimensionX;

	public int gridDimensionY;

	public GridSettings(CustomGridGraph graph)
	{
		Vector3 vector = graph.center - new Vector3(graph.size.x, 0f, graph.size.y) / 2f;
		cellSize = graph.nodeSize;
		positionOffset = new float2(vector.x, vector.z);
		gridDimensionX = graph.width;
		gridDimensionY = graph.depth;
	}
}
