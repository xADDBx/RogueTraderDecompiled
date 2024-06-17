using System;
using System.Collections.Generic;
using Pathfinding;

namespace Kingmaker.Pathfinding.LosCaching;

public class LosCacheContainer<TLosCache> : LosCache where TLosCache : LosCache
{
	private class AreaPartRect
	{
		public uint area;

		public IntRect rect;

		public static IntRect UndefinedRect = new IntRect(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);

		public AreaPartRect(uint area)
			: this(area, UndefinedRect)
		{
		}

		public AreaPartRect(uint area, IntRect rect)
		{
			this.area = area;
			this.rect = rect;
		}
	}

	private readonly Dictionary<uint, TLosCache> caches = new Dictionary<uint, TLosCache>();

	public LosCacheContainer(CustomGridGraph graph, IntRect bounds)
		: base(graph, bounds)
	{
		if (graph.nodes == null)
		{
			return;
		}
		List<AreaPartRect> list = new List<AreaPartRect>();
		CustomGridNode[] nodes = graph.nodes;
		foreach (CustomGridNode node in nodes)
		{
			if (bounds.Contains(node.XCoordinateInGrid, node.ZCoordinateInGrid) && node.Walkable)
			{
				AreaPartRect areaPartRect = list.Find((AreaPartRect x) => x.area == node.Area);
				if (areaPartRect == null)
				{
					areaPartRect = new AreaPartRect(node.Area);
					list.Add(areaPartRect);
				}
				areaPartRect.rect.xmin = Math.Min(areaPartRect.rect.xmin, node.XCoordinateInGrid);
				areaPartRect.rect.ymin = Math.Min(areaPartRect.rect.ymin, node.ZCoordinateInGrid);
				areaPartRect.rect.xmax = Math.Max(areaPartRect.rect.xmax, node.XCoordinateInGrid);
				areaPartRect.rect.ymax = Math.Max(areaPartRect.rect.ymax, node.ZCoordinateInGrid);
			}
		}
		foreach (AreaPartRect item in list)
		{
			IntRect rect = item.rect;
			if (rect.xmin <= rect.xmax && rect.ymin <= rect.ymax)
			{
				caches.Add(item.area, Create(graph, rect));
			}
		}
	}

	public override bool CheckLos(CustomGridNodeBase origin, IntRect originSize, CustomGridNodeBase end, IntRect endSize)
	{
		if (origin.Area != end.Area)
		{
			return false;
		}
		if (caches.TryGetValue(origin.Area, out var value))
		{
			return value.CheckLos(origin, originSize, end, endSize);
		}
		return false;
	}

	private TLosCache Create(CustomGridGraph graph, IntRect rect)
	{
		return (TLosCache)Activator.CreateInstance(typeof(TLosCache), graph, rect);
	}

	public override void DebugDraw()
	{
		foreach (TLosCache value in caches.Values)
		{
			value.DebugDraw();
		}
	}
}
