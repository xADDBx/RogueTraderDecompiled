using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Parts;

public static class PartPsykerExtension
{
	public static PartPsyker GetPsykerOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartPsyker>();
	}
}
