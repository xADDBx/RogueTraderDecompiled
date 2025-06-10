using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("bfcde29f5c2843aa9aeb292eb60fb20e")]
public class AbilityCasterDesiredPositionFromSelectedTarget : BlueprintComponent, IAbilityOverrideCasterDesiredPosition
{
	public bool TryGetDesiredPositionAndDirection(AbilityData abilityData, out Vector3 position, out Vector3 direction)
	{
		TargetWrapper targetWrapper = Game.Instance.SelectedAbilityHandler?.MultiTargetHandler.GetLastTarget();
		if (targetWrapper == null)
		{
			PFLog.Ability.Error(this, "No target was set");
			position = (direction = default(Vector3));
			return false;
		}
		position = targetWrapper.Point;
		direction = targetWrapper.Forward;
		return true;
	}
}
