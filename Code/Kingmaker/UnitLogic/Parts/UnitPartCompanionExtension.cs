using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Parts;

public static class UnitPartCompanionExtension
{
	[CanBeNull]
	public static UnitPartCompanion GetCompanionOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<UnitPartCompanion>();
	}
}
