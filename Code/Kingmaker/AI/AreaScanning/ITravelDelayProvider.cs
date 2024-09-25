using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;

namespace Kingmaker.AI.AreaScanning;

public interface ITravelDelayProvider
{
	PassInfo GetPassInfo(BaseUnitEntity unit, CustomGridNodeBase nodeFrom, CustomGridNodeBase nodeTo, bool isEvenDiagonal, bool isDiagonalDirection);
}
