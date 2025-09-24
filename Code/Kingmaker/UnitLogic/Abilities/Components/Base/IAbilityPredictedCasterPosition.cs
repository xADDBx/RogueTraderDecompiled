using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public interface IAbilityPredictedCasterPosition
{
	bool TryGetPredictedPositionAndDirection(AbilityData abilityData, out Vector3 position, out Vector3 direction);
}
