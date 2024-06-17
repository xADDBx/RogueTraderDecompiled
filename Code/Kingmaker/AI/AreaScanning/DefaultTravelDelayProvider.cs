using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;

namespace Kingmaker.AI.AreaScanning;

public class DefaultTravelDelayProvider : ITravelDelayProvider
{
	public PassInfo GetPassInfo(BaseUnitEntity unit, CustomGridNodeBase nodeFrom, CustomGridNodeBase nodeTo, bool isEvenDiagonal, bool isDiagonalDirection)
	{
		float num = unit.Blueprint.WarhammerMovementApPerCell * (float)((!isEvenDiagonal) ? 1 : 2);
		PassInfo result = default(PassInfo);
		result.pathCost = num;
		result.delay = num + (isDiagonalDirection ? 0.1f : 0f);
		result.enteredAoE = 0;
		result.provokedAoO = 0;
		return result;
	}
}
