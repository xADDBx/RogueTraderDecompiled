using Kingmaker.Mechanics.Entities;

namespace Kingmaker.UI.Selection.UnitMark.Managers;

public class UnitNpcMarkManager : UnitMarkManager
{
	protected override bool UnitNeedsMark(AbstractUnitEntity unit)
	{
		if (base.UnitNeedsMark(unit) && !unit.IsPlayerEnemy)
		{
			return !unit.IsPlayerFaction;
		}
		return false;
	}
}
