using Kingmaker.Pathfinding;
using Pathfinding;

namespace Kingmaker.UI.SurfaceCombatHUD;

internal static class CombatHudGraphDataSource
{
	public static CustomGridGraph FindGraph()
	{
		AstarPath active = AstarPath.active;
		if (active == null)
		{
			return null;
		}
		if (active.graphs == null)
		{
			return null;
		}
		NavGraph[] graphs = active.graphs;
		for (int i = 0; i < graphs.Length; i++)
		{
			if (graphs[i] is CustomGridGraph customGridGraph && IsGraphValid(customGridGraph))
			{
				return customGridGraph;
			}
		}
		return null;
	}

	public static bool IsGraphValid(CustomGridGraph graph)
	{
		if (graph == null)
		{
			return false;
		}
		if (graph.width <= 0)
		{
			return false;
		}
		if (graph.depth <= 0)
		{
			return false;
		}
		if (graph.nodes == null)
		{
			return false;
		}
		if (graph.meshNodes == null)
		{
			return false;
		}
		int num = graph.width * graph.depth;
		if (num != graph.nodes.Length)
		{
			return false;
		}
		if (num != graph.meshNodes.Length)
		{
			return false;
		}
		return true;
	}
}
