using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[ComponentName("Predicates/Target has fact")]
[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("d5cf2d061e9c403bb3ff39ff6f2ed4c3")]
public class AbilityTargetHasMeleeWeapon : BlueprintComponent, IAbilityTargetRestriction
{
	public bool Not;

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		bool flag = false;
		IList<HandsEquipmentSet> list = target.Entity?.GetBodyOptional()?.HandsEquipmentSets;
		if (list == null)
		{
			return Not;
		}
		foreach (HandsEquipmentSet item in list)
		{
			flag |= item.PrimaryHand.MaybeItem?.Blueprint is BlueprintItemWeapon { IsMelee: not false } || (item.SecondaryHand.MaybeItem?.Blueprint is BlueprintItemWeapon blueprintItemWeapon2 && blueprintItemWeapon2.IsMelee);
		}
		if (!Not)
		{
			return flag;
		}
		return !flag;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return Not ? BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetHasNoMeleeWeapon : BlueprintRoot.Instance.LocalizedTexts.Reasons.TargetHasMeleeWeapon;
	}
}
