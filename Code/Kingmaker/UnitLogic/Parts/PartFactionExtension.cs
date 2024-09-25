using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Parts;

public static class PartFactionExtension
{
	[CanBeNull]
	public static PartFaction GetFactionOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartFaction>();
	}
}
