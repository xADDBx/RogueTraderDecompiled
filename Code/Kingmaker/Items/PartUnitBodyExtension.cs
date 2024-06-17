using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Items;

public static class PartUnitBodyExtension
{
	[CanBeNull]
	public static PartUnitBody GetBodyOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartUnitBody>();
	}
}
