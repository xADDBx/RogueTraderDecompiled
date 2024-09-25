using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Inspect;

public static class InspectUnitsHelper
{
	public static UnitInspectInfoByPart GetInfo(BlueprintUnit unit, bool force = false)
	{
		return new UnitInspectInfoByPart(Game.Instance.Player.InspectUnitsManager.GetInfo(unit), force);
	}

	public static bool IsInspectAllow(MechanicEntity unit)
	{
		if (unit == null)
		{
			return false;
		}
		if (!unit.IsInCombat && !unit.IsPlayerFaction && !unit.IsHelpingPlayerFaction && !unit.IsPlayerEnemy && !unit.IsNeutral)
		{
			return unit.IsSummonedMonster;
		}
		return true;
	}
}
