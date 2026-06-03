using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Pathfinding;

namespace Kingmaker.View;

public class UnitsPlacer
{
	public static void MovePartyToNavmesh()
	{
		if (!Game.Instance.CurrentlyLoadedArea.IsPartyArea)
		{
			return;
		}
		foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
		{
			if (allUnit.IsDirectlyControllable)
			{
				NNInfo nNInfo = ObstacleAnalyzer.FindNearestNodeOnLevel(allUnit.Position, GraphParamsMechanicsCache.GridCellSize);
				allUnit.Position = ((nNInfo.node != null) ? nNInfo.position : ObstacleAnalyzer.GetNearestNode(allUnit.Position).position);
			}
		}
	}
}
