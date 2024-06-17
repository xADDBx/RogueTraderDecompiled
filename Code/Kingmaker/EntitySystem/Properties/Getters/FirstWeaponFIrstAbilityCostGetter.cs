using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Commands;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("8a8404a274de4de43bad5ac46eaa8259")]
public class FirstWeaponFIrstAbilityCostGetter : UnitPropertyGetter
{
	protected override string GetInnerCaption()
	{
		return "First ability AP cost";
	}

	protected override int GetBaseValue()
	{
		ItemEntityWeapon itemEntityWeapon = base.CurrentEntity.Body.CurrentHandsEquipmentSet.PrimaryHand.MaybeWeapon;
		if (base.CurrentEntity.Commands.Current is UnitUseAbility unitUseAbility)
		{
			itemEntityWeapon = unitUseAbility.Ability.Weapon;
		}
		return itemEntityWeapon?.Blueprint.WeaponAbilities.Ability1.AP ?? 0;
	}
}
