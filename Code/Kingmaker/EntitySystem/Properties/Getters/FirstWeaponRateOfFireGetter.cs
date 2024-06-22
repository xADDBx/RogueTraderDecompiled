using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("5f8f336ef74785c44862a41246f50a9b")]
public class FirstWeaponRateOfFireGetter : UnitPropertyGetter
{
	public bool ChosenWeapon;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "First weapon of " + FormulaTargetScope.Current + " Rate of Fire";
	}

	protected override int GetBaseValue()
	{
		ItemEntityWeapon itemEntityWeapon = base.CurrentEntity.GetOptional<WarhammerUnitPartChooseWeapon>()?.ChosenWeapon;
		ItemEntityWeapon maybeWeapon = base.CurrentEntity.Body.PrimaryHand.MaybeWeapon;
		if (maybeWeapon == null)
		{
			return 0;
		}
		if (ChosenWeapon && itemEntityWeapon == null)
		{
			return 0;
		}
		if (!ChosenWeapon)
		{
			return maybeWeapon.GetWeaponStats().ResultRateOfFire;
		}
		return itemEntityWeapon.GetWeaponStats().ResultRateOfFire;
	}
}
