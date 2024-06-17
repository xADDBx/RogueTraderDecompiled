using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;

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
		if (!unit.IsPlayerFaction && !unit.IsPlayerEnemy && !unit.IsNeutral)
		{
			return unit.GetOptional<UnitPartSummonedMonster>();
		}
		return true;
	}
}
