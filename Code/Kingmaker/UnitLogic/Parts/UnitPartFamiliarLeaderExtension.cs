using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Parts;

public static class UnitPartFamiliarLeaderExtension
{
	[CanBeNull]
	public static UnitPartFamiliarLeader GetFamiliarLeaderOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<UnitPartFamiliarLeader>();
	}
}
