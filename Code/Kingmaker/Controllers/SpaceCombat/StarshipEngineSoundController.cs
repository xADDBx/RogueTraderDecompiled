using Kingmaker.Controllers.Units;
using Kingmaker.Mechanics.Entities;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;

namespace Kingmaker.Controllers.SpaceCombat;

public class StarshipEngineSoundController : BaseUnitController
{
	protected override void TickOnUnit(AbstractUnitEntity unit)
	{
		unit.GetStarshipEngineOptional()?.UpdateEngineSound(unit.MovementAgent.IsReallyMoving);
	}
}
