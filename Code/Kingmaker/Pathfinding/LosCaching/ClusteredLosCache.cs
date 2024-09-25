using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Covers;
using Pathfinding;

namespace Kingmaker.Pathfinding.LosCaching;

public class ClusteredLosCache : LosCache
{
	private class LosCluster
	{
		private IntRect m_Rect;

		private bool m_CanGrowHorizontally;

		private bool m_CanGrowHorizontallyChecked;

		private bool m_CanGrowVertically;

		private bool m_CanGrowVerticallyChecked;

		public CustomGridGraph Graph { get; }

		public IntRect Bounds { get; }

		public IntRect Rect => m_Rect;

		public bool CanGrowHorizontally
		{
			get
			{
				if (m_CanGrowHorizontallyChecked)
				{
					return m_CanGrowHorizontally;
				}
				int num = Rect.xmax + 1;
				if (num > Bounds.xmax)
				{
					m_CanGrowHorizontallyChecked = true;
					m_CanGrowHorizontally = false;
					return m_CanGrowHorizontally;
				}
				for (int i = Rect.ymin; i <= Rect.ymax; i++)
				{
					CustomGridNodeBase node = Graph.GetNode(num, i);
					if (node == null || !node.Walkable)
					{
						continue;
					}
					for (int j = Rect.xmin; j <= Rect.xmax; j++)
					{
						for (int k = Rect.ymin; k <= Rect.ymax; k++)
						{
							CustomGridNodeBase node2 = Graph.GetNode(j, k);
							if (node2 != null && node2.Walkable && (!LosCalculations.HasLos(node, default(IntRect), node2, default(IntRect)) || !LosCalculations.HasLos(node2, default(IntRect), node, default(IntRect))))
							{
								m_CanGrowHorizontallyChecked = true;
								m_CanGrowHorizontally = false;
								return m_CanGrowHorizontally;
							}
						}
					}
				}
				m_CanGrowHorizontallyChecked = true;
				m_CanGrowHorizontally = true;
				return m_CanGrowHorizontally;
			}
		}

		public bool CanGrowVertically
		{
			get
			{
				if (m_CanGrowVerticallyChecked)
				{
					return m_CanGrowVertically;
				}
				int num = Rect.ymax + 1;
				if (num > Bounds.ymax)
				{
					m_CanGrowVerticallyChecked = true;
					m_CanGrowVertically = false;
					return m_CanGrowVertically;
				}
				for (int i = Rect.xmin; i <= Rect.xmax; i++)
				{
					CustomGridNodeBase node = Graph.GetNode(i, num);
					if (node == null || !node.Walkable)
					{
						continue;
					}
					for (int j = Rect.xmin; j <= Rect.xmax; j++)
					{
						for (int k = Rect.ymin; k <= Rect.ymax; k++)
						{
							CustomGridNodeBase node2 = Graph.GetNode(j, k);
							if (node2 != null && node2.Walkable && (!LosCalculations.HasLos(node, default(IntRect), node2, default(IntRect)) || !LosCalculations.HasLos(node2, default(IntRect), node, default(IntRect))))
							{
								m_CanGrowVerticallyChecked = true;
								m_CanGrowVertically = false;
								return m_CanGrowVertically;
							}
						}
					}
				}
				m_CanGrowVerticallyChecked = true;
				m_CanGrowVertically = true;
				return m_CanGrowVertically;
			}
		}

		public LosCluster(CustomGridGraph graph, IntRect rect, IntRect bounds)
		{
			Graph = graph;
			Bounds = bounds;
			m_Rect = rect;
		}

		public void GrowHorizontally()
		{
			m_Rect.xmax++;
			m_CanGrowHorizontallyChecked = false;
			m_CanGrowVerticallyChecked = false;
		}

		public void GrowVertically()
		{
			m_Rect.ymax++;
			m_CanGrowHorizontallyChecked = false;
			m_CanGrowVerticallyChecked = false;
		}

		public bool Contains(LosCluster other)
		{
			if (Rect.Contains(other.Rect.xmin, other.Rect.ymin))
			{
				return Rect.Contains(other.Rect.xmax, other.Rect.ymax);
			}
			return false;
		}

		public bool Contains(int x, int y)
		{
			return Rect.Contains(x, y);
		}

		public override string ToString()
		{
			return $"Cluster {Rect}";
		}
	}

	private List<LosCluster> clusters;

	public ClusteredLosCache(CustomGridGraph graph, IntRect bounds)
		: base(graph, bounds)
	{
		clusters = new List<LosCluster>();
		CustomGridQuadtree customGridQuadtree = new CustomGridQuadtree(graph, bounds);
		customGridQuadtree.DivideDown();
		customGridQuadtree.ConsolidateUpPairwise(LosCache.CanConsolidateByLos, 32);
		InitClusters(customGridQuadtree, bounds);
		RemoveEmptyClusters();
		int num = 0;
		while (CanGrowClusters() && num < 10000)
		{
			GrowClusters();
			num++;
		}
		SortClusters();
	}

	private void InitClusters(CustomGridQuadtree treeNode, IntRect bounds)
	{
		if (treeNode != null)
		{
			if (treeNode.IsLeaf)
			{
				clusters.Add(new LosCluster(graph, treeNode.Rect, bounds));
			}
			InitClusters(treeNode.LeftTopChild, bounds);
			InitClusters(treeNode.RightTopChild, bounds);
			InitClusters(treeNode.LeftBottomChild, bounds);
			InitClusters(treeNode.RightBottomChild, bounds);
		}
	}

	private void RemoveEmptyClusters()
	{
		List<LosCluster> toRemove = new List<LosCluster>();
		foreach (LosCluster cluster in clusters)
		{
			if (IsEmptyCluster(cluster))
			{
				toRemove.Add(cluster);
			}
		}
		clusters.Remove((LosCluster x) => toRemove.Contains(x));
	}

	private bool IsEmptyCluster(LosCluster c)
	{
		for (int i = c.Rect.xmin; i <= c.Rect.xmax; i++)
		{
			for (int j = c.Rect.ymin; j <= c.Rect.ymax; j++)
			{
				CustomGridNodeBase node = graph.GetNode(i, j);
				if (node != null && node.Walkable)
				{
					return false;
				}
			}
		}
		return true;
	}

	private bool CanGrowClusters()
	{
		foreach (LosCluster cluster in clusters)
		{
			if (cluster.CanGrowHorizontally)
			{
				return true;
			}
			if (cluster.CanGrowVertically)
			{
				return true;
			}
		}
		return false;
	}

	private void GrowClusters()
	{
		foreach (LosCluster cluster in clusters)
		{
			if (cluster.CanGrowHorizontally)
			{
				cluster.GrowHorizontally();
			}
			else if (cluster.CanGrowVertically)
			{
				cluster.GrowVertically();
			}
		}
		List<LosCluster> toRemove = new List<LosCluster>();
		for (int i = 0; i < clusters.Count - 1; i++)
		{
			LosCluster losCluster = clusters[i];
			for (int j = i + 1; j < clusters.Count; j++)
			{
				LosCluster losCluster2 = clusters[j];
				if (losCluster.Contains(losCluster2))
				{
					toRemove.Add(losCluster2);
				}
				else if (losCluster2.Contains(losCluster))
				{
					toRemove.Add(losCluster);
				}
			}
		}
		clusters.Remove((LosCluster x) => toRemove.Contains(x));
	}

	private void SortClusters()
	{
		clusters.Sort((LosCluster x, LosCluster y) => (x.Rect.xmin == y.Rect.xmin) ? (x.Rect.ymin - y.Rect.ymin) : (x.Rect.xmin - y.Rect.xmin));
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

	public bool CheckLos(CustomGridNodeBase from, CustomGridNodeBase to)
	{
		int xCoordinateInGrid = from.XCoordinateInGrid;
		int zCoordinateInGrid = from.ZCoordinateInGrid;
		foreach (LosCluster cluster in clusters)
		{
			if (cluster.Rect.xmin <= xCoordinateInGrid && cluster.Rect.ymin <= zCoordinateInGrid)
			{
				if (cluster.Rect.xmax < xCoordinateInGrid || cluster.Rect.ymax < zCoordinateInGrid)
				{
					break;
				}
				if (cluster.Contains(to.XCoordinateInGrid, to.ZCoordinateInGrid))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void DebugDraw()
	{
		foreach (LosCluster cluster in clusters)
		{
			DrawRect(cluster.Rect);
		}
	}
}
