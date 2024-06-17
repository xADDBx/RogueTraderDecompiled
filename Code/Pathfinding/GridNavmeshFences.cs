using System;
using System.Linq;
using Kingmaker.Pathfinding;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

public readonly struct GridNavmeshFences
{
	private readonly GridNavmeshFence[] m_Fences;

	private GridNavmeshFences(GridNavmeshFence[] fences)
	{
		m_Fences = fences;
		Array.Sort(m_Fences, (GridNavmeshFence a, GridNavmeshFence b) => b.height - a.height);
	}

	public static GridNavmeshFences Create(GraphTransform transform)
	{
		return new GridNavmeshFences((from v in NavmeshClipper.allEnabled
			where v is ThinCover
			select new GridNavmeshFence(v.GetBounds(transform), (int)(((ThinCover)v).Top * 1000f))).ToArray());
	}

	public bool IsFenceBetween(CustomGridNodeBase node1, CustomGridNodeBase node2)
	{
		GridNavmeshFence? fence;
		return IsFenceBetween(node1, node2, out fence);
	}

	public bool IsFenceBetween(CustomGridNodeBase node1, CustomGridNodeBase node2, out GridNavmeshFence? fence)
	{
		fence = GetFenceBetween(node1, node2);
		return fence.HasValue;
	}

	public GridNavmeshFence? GetFenceBetween(CustomGridNodeBase from, CustomGridNodeBase to)
	{
		Rect nodeRect = GetNodeRect(from);
		Rect nodeRect2 = GetNodeRect(to);
		for (int i = 0; i < m_Fences.Length; i++)
		{
			if (m_Fences[i].bounds.Overlaps(nodeRect) && m_Fences[i].bounds.Overlaps(nodeRect2) && IsFenceBetween(m_Fences[i].bounds, nodeRect, nodeRect2))
			{
				return m_Fences[i];
			}
		}
		return null;
	}

	private Rect GetNodeRect(CustomGridNodeBase node)
	{
		int xCoordinateInGrid = node.XCoordinateInGrid;
		int zCoordinateInGrid = node.ZCoordinateInGrid;
		return new Rect(new Vector2(xCoordinateInGrid, zCoordinateInGrid), new Vector2(1f, 1f));
	}

	private bool IsFenceBetween(Rect cover, Rect nodeRect1, Rect nodeRect2)
	{
		bool num = cover.width > cover.height;
		Vector2 vector = nodeRect1.center - nodeRect2.center;
		Vector2 center = nodeRect1.center;
		Vector2 center2 = nodeRect2.center;
		if (!num)
		{
			if (Math.Abs(vector.x) > 0.5f)
			{
				if (!(center.y < cover.yMax) || !(center.y > cover.yMin))
				{
					if (center2.y < cover.yMax)
					{
						return center2.y > cover.yMin;
					}
					return false;
				}
				return true;
			}
			return false;
		}
		if (Math.Abs(vector.y) > 0.5f)
		{
			if (!(center.x < cover.xMax) || !(center.x > cover.xMin))
			{
				if (center2.x < cover.xMax)
				{
					return center2.x > cover.xMin;
				}
				return false;
			}
			return true;
		}
		return false;
	}
}
