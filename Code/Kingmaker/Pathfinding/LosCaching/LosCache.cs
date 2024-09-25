using Kingmaker.Utility.CodeTimer;
using Kingmaker.View.Covers;
using Pathfinding;

namespace Kingmaker.Pathfinding.LosCaching;

public abstract class LosCache
{
	protected readonly CustomGridGraph graph;

	protected readonly IntRect bounds;

	public LosCache(CustomGridGraph graph, IntRect bounds)
	{
		this.graph = graph;
		this.bounds = bounds;
	}

	public abstract bool CheckLos(CustomGridNodeBase origin, IntRect originSize, CustomGridNodeBase end, IntRect endSize);

	public static bool CanConsolidateByLos(CustomGridQuadtree lt, CustomGridQuadtree rt, CustomGridQuadtree lb, CustomGridQuadtree rb)
	{
		if (CanConsolidateByLos(lt, rt) && CanConsolidateByLos(lt, lb) && CanConsolidateByLos(lt, rb) && CanConsolidateByLos(rt, lb) && CanConsolidateByLos(rt, rb))
		{
			return CanConsolidateByLos(lb, rb);
		}
		return false;
	}

	public static bool CanConsolidateByLos(CustomGridQuadtree q1, CustomGridQuadtree q2)
	{
		for (int i = q1.Rect.xmin; i <= q1.Rect.xmax; i++)
		{
			for (int j = q1.Rect.ymin; j <= q1.Rect.ymax; j++)
			{
				CustomGridNodeBase node = q1.Graph.GetNode(i, j);
				if (node == null || !node.Walkable)
				{
					continue;
				}
				for (int k = q2.Rect.xmin; k <= q2.Rect.xmax; k++)
				{
					for (int l = q2.Rect.ymin; l <= q2.Rect.ymax; l++)
					{
						CustomGridNodeBase node2 = q2.Graph.GetNode(k, l);
						if (node2 != null && node2.Walkable && !HasTwoWayLos(node, node2))
						{
							return false;
						}
					}
				}
			}
		}
		return true;
	}

	private static bool HasTwoWayLos(CustomGridNodeBase node1, CustomGridNodeBase node2)
	{
		using (ProfileScope.New("HasLos"))
		{
			return LosCalculations.HasLos(node1, default(IntRect), node2, default(IntRect)) && LosCalculations.HasLos(node2, default(IntRect), node1, default(IntRect));
		}
	}

	public virtual void DebugDraw()
	{
	}

	protected virtual void DrawRect(IntRect rect)
	{
	}
}
