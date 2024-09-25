using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Parts;

public static class PartMomentumExtension
{
	public static PartMomentum GetMomentumOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartMomentum>();
	}
}
