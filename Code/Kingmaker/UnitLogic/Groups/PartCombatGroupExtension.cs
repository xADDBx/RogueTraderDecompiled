using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Groups;

public static class PartCombatGroupExtension
{
	[CanBeNull]
	public static PartCombatGroup GetCombatGroupOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartCombatGroup>();
	}
}
