using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

public static class StrategistKeystoneRearAbilityFlipPatternExt
{
	public static Vector3 ApplyFlipPattern(this MechanicEntity entity, AbilityData abilityData, Vector3 @default)
	{
		Vector3 result = @default;
		if ((object)abilityData != null && abilityData.Blueprint.HasLogic<IsFlipZoneAbility>())
		{
			UnitPartStrategistKeystoneRearAbilityFlipPattern optional = entity.GetOptional<UnitPartStrategistKeystoneRearAbilityFlipPattern>();
			if (optional != null)
			{
				result = optional.GetOverrideDirection();
			}
		}
		return result;
	}

	public static Vector3 ApplyFlipPattern(this AreaEffectEntity entity, Vector3 @default)
	{
		return (entity.Context?.MaybeCaster?.GetOptional<UnitPartStrategistKeystoneRearAbilityFlipPattern>())?.GetOverrideDirection(entity.UniqueId, @default) ?? @default;
	}
}
