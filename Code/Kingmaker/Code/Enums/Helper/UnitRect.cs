using Pathfinding;
using UnityEngine;

namespace Kingmaker.Code.Enums.Helper;

public readonly struct UnitRect
{
	private readonly Vector2Int m_Position;

	private readonly IntRect m_Bounds;

	public int Left => m_Position.x + m_Bounds.xmin;

	public int Right => m_Position.x + m_Bounds.xmax;

	public int Top => m_Position.y + m_Bounds.ymax;

	public int Bottom => m_Position.y + m_Bounds.ymin;

	public UnitRect(Vector2Int position, IntRect bounds)
	{
		m_Position = position;
		m_Bounds = bounds;
	}
}
