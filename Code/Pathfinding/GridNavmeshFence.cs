using UnityEngine;

namespace Pathfinding;

public readonly struct GridNavmeshFence
{
	public readonly Rect bounds;

	public readonly int height;

	public GridNavmeshFence(Rect bounds, int height)
	{
		this.bounds = bounds;
		this.height = height;
	}
}
