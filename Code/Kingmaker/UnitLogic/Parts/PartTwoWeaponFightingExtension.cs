using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Parts;

public static class PartTwoWeaponFightingExtension
{
	[CanBeNull]
	public static PartTwoWeaponFighting GetTwoWeaponFightingOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartTwoWeaponFighting>();
	}
}
