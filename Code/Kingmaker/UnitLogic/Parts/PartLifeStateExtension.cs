using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Parts;

public static class PartLifeStateExtension
{
	public static PartLifeState GetLifeStateOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartLifeState>();
	}
}
