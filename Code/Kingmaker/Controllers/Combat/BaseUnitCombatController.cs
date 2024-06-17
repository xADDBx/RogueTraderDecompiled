using Kingmaker.Controllers.Units;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Controllers.Combat;

public abstract class BaseUnitCombatController : BaseUnitController
{
	protected override bool ShouldTickOnUnit(AbstractUnitEntity unit)
	{
		if (base.ShouldTickOnUnit(unit))
		{
			return unit.IsInCombat;
		}
		return false;
	}
}
