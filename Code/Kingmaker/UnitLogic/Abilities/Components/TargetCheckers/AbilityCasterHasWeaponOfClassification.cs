using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility.Attributes;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[ComponentName("Predicates/Target has fact")]
[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("68b70115a9624633af295d7e88c188ab")]
public class AbilityCasterHasWeaponOfClassification : BlueprintComponent, IAbilityCasterRestriction
{
	public WeaponClassification Classification;

	public bool CheckCurrentSet;

	[HideIf("CheckOnlySecondaryHand")]
	public bool CheckOnlyPrimaryHand;

	[FormerlySerializedAs("CheckOnySecondaryHand")]
	[HideIf("CheckOnlyPrimaryHand")]
	public bool CheckOnlySecondaryHand;

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		bool flag = false;
		IList<HandsEquipmentSet> list = caster.GetBodyOptional()?.HandsEquipmentSets;
		if (list == null)
		{
			return false;
		}
		ItemEntityWeapon itemEntityWeapon = null;
		ItemEntityWeapon itemEntityWeapon2 = null;
		if (!CheckCurrentSet)
		{
			foreach (HandsEquipmentSet item in list)
			{
				itemEntityWeapon = item.PrimaryHand.MaybeWeapon;
				itemEntityWeapon2 = item.SecondaryHand.MaybeWeapon;
				flag |= (!CheckOnlySecondaryHand && itemEntityWeapon != null && itemEntityWeapon.Blueprint.Classification == Classification) || (!CheckOnlyPrimaryHand && itemEntityWeapon2 != null && itemEntityWeapon2.Blueprint.Classification == Classification);
			}
			return flag;
		}
		itemEntityWeapon = caster.GetBodyOptional()?.PrimaryHand.MaybeWeapon;
		itemEntityWeapon2 = caster.GetBodyOptional()?.SecondaryHand.MaybeWeapon;
		return flag | ((!CheckOnlySecondaryHand && itemEntityWeapon != null && itemEntityWeapon.Blueprint.Classification == Classification) || (!CheckOnlyPrimaryHand && itemEntityWeapon2 != null && itemEntityWeapon2.Blueprint.Classification == Classification));
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		return BlueprintRoot.Instance.LocalizedTexts.Reasons.CasterHasNoWeaponOfClassification;
	}
}
