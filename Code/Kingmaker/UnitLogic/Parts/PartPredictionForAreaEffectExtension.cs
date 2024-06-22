using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Parts;

public static class PartPredictionForAreaEffectExtension
{
	[CanBeNull]
	public static PartAbilityPredictionForAreaEffect GetPartAbilityPredictionForAreaEffectOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartAbilityPredictionForAreaEffect>();
	}
}
