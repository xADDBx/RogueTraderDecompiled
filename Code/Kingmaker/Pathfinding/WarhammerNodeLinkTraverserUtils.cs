namespace Kingmaker.Pathfinding;

public static class WarhammerNodeLinkTraverserUtils
{
	public static bool IsTraverseState(this WarhammerNodeLinkTraverser.State state)
	{
		if ((uint)(state - 2) <= 4u)
		{
			return true;
		}
		return false;
	}
}
