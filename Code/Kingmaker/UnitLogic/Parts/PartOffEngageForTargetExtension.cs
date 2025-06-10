using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Parts;

public static class PartOffEngageForTargetExtension
{
	public static bool IsOffEngageForTarget(this BaseUnitEntity unit, BaseUnitEntity target)
	{
		return unit.GetOptional<PartOffEngageForTarget>()?.IsOffEngageForTarget(target) ?? false;
	}
}
