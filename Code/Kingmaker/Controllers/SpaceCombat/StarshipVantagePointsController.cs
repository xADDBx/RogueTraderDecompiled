using Kingmaker.Controllers.Units;
using Kingmaker.Mechanics.Entities;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;

namespace Kingmaker.Controllers.SpaceCombat;

public class StarshipVantagePointsController : BaseUnitController
{
	protected override void TickOnUnit(AbstractUnitEntity unit)
	{
		unit.GetOrCreate<PartVantagePoints>().UpdateIsInVantagePoint();
	}
}
