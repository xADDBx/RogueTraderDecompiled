using System;
using Pathfinding;

namespace Kingmaker.Pathfinding.LosCaching;

public class QuadtreeLosCache : LosCache
{
	private readonly CustomGridQuadtree quadtree;

	public QuadtreeLosCache(CustomGridGraph graph, IntRect bounds)
		: base(graph, bounds)
	{
		quadtree = new CustomGridQuadtree(graph, bounds);
		if (!IsFlatGraph(graph, bounds))
		{
			quadtree.DivideDown();
			quadtree.ConsolidateUpPairwise(LosCache.CanConsolidateByLos, 32);
		}
	}

	private bool IsFlatGraph(CustomGridGraph graph, IntRect bounds)
	{
		float num = float.MaxValue;
		float num2 = float.MinValue;
		float num3 = 1f;
		CustomGridNode[] nodes = graph.nodes;
		foreach (CustomGridNode customGridNode in nodes)
		{
			if (bounds.Contains(customGridNode.XCoordinateInGrid, customGridNode.ZCoordinateInGrid))
			{
				num = Math.Min(num, customGridNode.Vector3Position.y);
				num2 = Math.Max(num2, customGridNode.Vector3Position.y);
				if (num2 - num > num3)
				{
					return false;
				}
			}
		}
		return true;
	}

	public override bool CheckLos(CustomGridNodeBase origin, IntRect originSize, CustomGridNodeBase end, IntRect endSize)
	{
		for (int i = originSize.xmin; i <= originSize.xmax; i++)
		{
			for (int j = originSize.ymin; j <= originSize.ymax; j++)
			{
				CustomGridNodeBase node = graph.GetNode(origin.XCoordinateInGrid + i, origin.ZCoordinateInGrid + j);
				for (int k = endSize.xmin; k <= endSize.xmax; k++)
				{
					for (int l = endSize.ymin; l <= endSize.ymax; l++)
					{
						CustomGridNodeBase node2 = graph.GetNode(end.XCoordinateInGrid + k, end.ZCoordinateInGrid + l);
						if (CheckLos(node, node2))
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	public bool CheckLos(CustomGridNodeBase origin, CustomGridNodeBase end)
	{
		return quadtree.Find(origin.XCoordinateInGrid, origin.ZCoordinateInGrid)?.Contains(end.XCoordinateInGrid, end.ZCoordinateInGrid) ?? false;
	}

	public override void DebugDraw()
	{
		DrawQuadtree(quadtree);
	}

	private void DrawQuadtree(CustomGridQuadtree tree)
	{
		if (tree != null)
		{
			if (tree.IsLeaf)
			{
				DrawRect(tree.Rect);
			}
			DrawQuadtree(tree.LeftTopChild);
			DrawQuadtree(tree.RightTopChild);
			DrawQuadtree(tree.LeftBottomChild);
			DrawQuadtree(tree.RightBottomChild);
		}
	}
}
