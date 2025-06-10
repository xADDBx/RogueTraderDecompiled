using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public interface IAbilityOverrideCasterDesiredPosition
{
	bool TryGetDesiredPositionAndDirection(AbilityData abilityData, out Vector3 position, out Vector3 direction);
}
