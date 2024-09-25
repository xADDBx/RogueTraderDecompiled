using System;
using Kingmaker.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UI.SurfaceCombatHUD;

public sealed class RingAreaSource : IAreaSource
{
	private struct Writer<T> : GridPatterns.ILineWriter<T> where T : struct, IIdentifierContainer
	{
		private Vector2Int m_GridDimensions;

		public Writer(Vector2Int gridDimensions)
		{
			m_GridDimensions = gridDimensions;
		}

		public void Write(ref T container, int xBegin, int xEnd, int y)
		{
			if (y >= 0 && y < m_GridDimensions.y)
			{
				if (xBegin < 0)
				{
					xBegin = 0;
				}
				if (xEnd > m_GridDimensions.x)
				{
					xEnd = m_GridDimensions.x;
				}
				int num = y * m_GridDimensions.x;
				container.PushRange(num + xBegin, num + xEnd);
			}
		}
	}

	private IntRect m_UnitRect;

	private int m_InnerRadius;

	private int m_OuterRadius;

	public void Setup(IntRect unitRect, int innerRadius, int outerRadius)
	{
		m_UnitRect = unitRect;
		m_InnerRadius = innerRadius;
		m_OuterRadius = outerRadius;
	}

	public int EstimateCount()
	{
		return Mathf.CeilToInt(MathF.PI * (float)(m_OuterRadius * m_OuterRadius - m_InnerRadius * m_InnerRadius));
	}

	public void Clear()
	{
		m_UnitRect = default(IntRect);
		m_InnerRadius = 0;
		m_OuterRadius = 0;
	}

	public void GetCellIdentifiers<T>(Vector2Int gridDimensions, ref T container) where T : struct, IIdentifierContainer
	{
		Writer<T> writer = new Writer<T>(gridDimensions);
		GridPatterns.GenerateRoundedRectangle(m_InnerRadius, m_OuterRadius, m_UnitRect.xmin, m_UnitRect.ymin, m_UnitRect.Width, m_UnitRect.Height, ref container, ref writer);
	}
}
