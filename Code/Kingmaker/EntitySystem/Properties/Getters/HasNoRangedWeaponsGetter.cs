using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("250de3495a5143389abd428fcfd0325d")]
public class HasNoRangedWeaponsGetter : UnitPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Both weapons of " + FormulaTargetScope.Current + " are not ranged";
	}

	protected override int GetBaseValue()
	{
		foreach (HandsEquipmentSet handsEquipmentSet in base.CurrentEntity.Body.HandsEquipmentSets)
		{
			ItemEntityWeapon maybeWeapon = handsEquipmentSet.PrimaryHand.MaybeWeapon;
			if (maybeWeapon != null && maybeWeapon.Blueprint.IsRanged)
			{
				return 0;
			}
			ItemEntityWeapon maybeWeapon2 = handsEquipmentSet.SecondaryHand.MaybeWeapon;
			if (maybeWeapon2 != null && maybeWeapon2.Blueprint.IsRanged)
			{
				return 0;
			}
		}
		return 1;
	}
}
