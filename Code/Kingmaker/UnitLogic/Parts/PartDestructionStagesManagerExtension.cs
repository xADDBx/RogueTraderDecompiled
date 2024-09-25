using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Parts;

public static class PartDestructionStagesManagerExtension
{
	[CanBeNull]
	public static PartDestructionStagesManager GetDestructionStagesManagerOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartDestructionStagesManager>();
	}
}
