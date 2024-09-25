using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Parts;

public static class UnitPartFamiliarExtensions
{
	public static UnitPartFamiliar GetFamiliarOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<UnitPartFamiliar>();
	}
}
