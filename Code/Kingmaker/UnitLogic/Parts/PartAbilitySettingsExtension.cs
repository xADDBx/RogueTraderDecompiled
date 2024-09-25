using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Parts;

public static class PartAbilitySettingsExtension
{
	[CanBeNull]
	public static PartAbilitySettings GetAbilitySettingsOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartAbilitySettings>();
	}
}
