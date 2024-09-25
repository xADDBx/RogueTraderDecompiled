using Kingmaker.Mechanics.Entities;

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
				allUnit.Position = ObstacleAnalyzer.GetNearestNode(allUnit.Position).position;
			}
		}
	}
}
