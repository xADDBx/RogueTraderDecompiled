using Pathfinding;

namespace Kingmaker.Pathfinding;

public static class IntRectEx
{
	public static bool Contains(this IntRect rect, IntRect other)
	{
		if (rect.Contains(other.xmin, other.ymin))
		{
			return rect.Contains(other.xmax, other.ymax);
		}
		return false;
	}
}
