using Kingmaker.Mechanics.Entities;

namespace Kingmaker.UI.Selection.UnitMark.Managers;

public class UnitEnemyMarkManager : UnitMarkManager
{
	protected override bool UnitNeedsMark(AbstractUnitEntity unit)
	{
		if (base.UnitNeedsMark(unit))
		{
			return unit.IsPlayerEnemy;
		}
		return false;
	}
}
