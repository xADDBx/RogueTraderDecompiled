using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Parts;

public static class PartAbilityPatternSettingsExtension
{
	[CanBeNull]
	public static PartAbilityPatternSettings GetAbilityPatternSettingsOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartAbilityPatternSettings>();
	}
}
