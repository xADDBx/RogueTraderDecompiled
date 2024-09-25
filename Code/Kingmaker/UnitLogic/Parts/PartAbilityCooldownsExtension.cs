using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Parts;

public static class PartAbilityCooldownsExtension
{
	[CanBeNull]
	public static PartAbilityCooldowns GetAbilityCooldownsOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartAbilityCooldowns>();
	}
}
