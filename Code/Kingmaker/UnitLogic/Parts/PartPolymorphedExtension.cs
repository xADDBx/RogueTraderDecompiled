using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Parts;

public static class PartPolymorphedExtension
{
	public static PartPolymorphed GetPolymorphedOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartPolymorphed>();
	}
}
