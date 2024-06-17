using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.UnitLogic.Parts;

public static class PartStatsAttributesExtension
{
	[CanBeNull]
	public static PartStatsAttributes GetAttributesOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartStatsAttributes>();
	}

	public static PartStatsAttributes GetAttributesOptional(this IMechanicEntity entity)
	{
		return ((MechanicEntity)entity).GetAttributesOptional();
	}
}
