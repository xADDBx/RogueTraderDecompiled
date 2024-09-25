using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Groups;

namespace Kingmaker.UnitLogic;

public static class UnitStealthExtension
{
	public static bool InStealthFor(this BaseUnitEntity unit, UnitGroup observer)
	{
		if (unit.Stealth.Active)
		{
			return !unit.Stealth.IsSpottedBy(observer);
		}
		return false;
	}
}
