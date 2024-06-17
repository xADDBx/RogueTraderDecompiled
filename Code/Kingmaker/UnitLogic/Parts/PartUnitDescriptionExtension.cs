using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Parts;

public static class PartUnitDescriptionExtension
{
	[CanBeNull]
	public static PartUnitDescription GetDescriptionOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartUnitDescription>();
	}
}
