using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic;

public static class PartUnitProficiencyExtension
{
	[CanBeNull]
	public static PartUnitProficiency GetProficienciesOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartUnitProficiency>();
	}
}
