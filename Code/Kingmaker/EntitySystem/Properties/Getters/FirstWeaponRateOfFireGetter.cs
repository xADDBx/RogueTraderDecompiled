using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Commands;
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
		int? num = base.CurrentEntity.GetPrimaryHandWeapon()?.GetWeaponStats().ResultRateOfFire;
		int? num2 = base.CurrentEntity.GetSecondaryHandWeapon()?.GetWeaponStats().ResultRateOfFire;
		int? num3 = ((num.GetValueOrDefault() >= num2.GetValueOrDefault()) ? num : num2);
		if (base.CurrentEntity.Commands.Current is UnitUseAbility unitUseAbility)
		{
			ItemEntityWeapon weapon = unitUseAbility.Ability.Weapon;
			num3 = ((weapon != null) ? new int?(weapon.GetWeaponStats().ResultRateOfFire) : num3);
		}
		if (!num3.HasValue)
		{
			return 0;
		}
		if (ChosenWeapon && itemEntityWeapon == null)
		{
			return 0;
		}
		if (!ChosenWeapon)
		{
			return num3.Value;
		}
		return itemEntityWeapon.GetWeaponStats().ResultRateOfFire;
	}
}
