using Kingmaker.Pathfinding;
using UnityEngine;

namespace Kingmaker.UI.SurfaceCombatHUD;

public class GridHeightTextureGeneratorUtil
{
	public static Texture2D Generate(CustomGridGraph graph, float impassableTerrainHeight)
	{
		if (graph == null)
		{
			return null;
		}
		Texture2D texture2D = new Texture2D(graph.width, graph.depth, TextureFormat.RFloat, mipChain: false, linear: true);
		texture2D.filterMode = FilterMode.Point;
		Color color = new Color(impassableTerrainHeight, impassableTerrainHeight, impassableTerrainHeight, impassableTerrainHeight);
		CustomGridNode[] nodes = graph.nodes;
		if (nodes == null || nodes.Length == 0)
		{
			return null;
		}
		foreach (CustomGridNode customGridNode in nodes)
		{
			if (customGridNode.Walkable)
			{
				Vector3 vector = (Vector3)customGridNode.position;
				texture2D.SetPixel(color: new Color(vector.y, vector.y, vector.y, vector.y), x: customGridNode.XCoordinateInGrid, y: customGridNode.ZCoordinateInGrid);
			}
			else
			{
				texture2D.SetPixel(customGridNode.XCoordinateInGrid, customGridNode.ZCoordinateInGrid, color);
			}
		}
		texture2D.Apply();
		return texture2D;
	}
}
