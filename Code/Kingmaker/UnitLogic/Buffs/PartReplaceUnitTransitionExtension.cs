using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Buffs;

public static class PartReplaceUnitTransitionExtension
{
	[CanBeNull]
	public static PartReplaceUnitTransition GetReplaceUnitTransitionOptional([CanBeNull] this MechanicEntity entity)
	{
		return entity?.GetOptional<PartReplaceUnitTransition>();
	}
}
