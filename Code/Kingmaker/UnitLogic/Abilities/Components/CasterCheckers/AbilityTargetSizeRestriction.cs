using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("7a7b0d6d0475eec458b26ba01752c36c")]
public class AbilityTargetSizeRestriction : BlueprintComponent, IAbilityTargetRestriction
{
	public Size[] AllowedSizes;

	public Size[] ForbiddenSizes;

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		StarshipEntity starship = target.Entity as StarshipEntity;
		if (starship == null)
		{
			return false;
		}
		if (AllowedSizes.Length != 0 && !AllowedSizes.Any((Size sz) => sz == starship.Blueprint.Size))
		{
			return false;
		}
		if (ForbiddenSizes.Any((Size sz) => sz == starship.Blueprint.Size))
		{
			return false;
		}
		return true;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return LocalizedTexts.Instance.Reasons.TargetIsInvalid;
	}
}
