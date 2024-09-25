using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UI.Models.UnitSettings;

public static class PartUnitUISettingsExtension
{
	[CanBeNull]
	public static PartUnitUISettings GetUnitUISettingsOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartUnitUISettings>();
	}
}
