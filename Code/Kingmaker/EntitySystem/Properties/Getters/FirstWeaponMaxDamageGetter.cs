using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Commands;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("4c8cb68a4355e444e87f858307f151c0")]
public class FirstWeaponMaxDamageGetter : UnitPropertyGetter
{
	protected override string GetInnerCaption()
	{
		return "First weapon Max Damage";
	}

	protected override int GetBaseValue()
	{
		ItemEntityWeapon itemEntityWeapon = base.CurrentEntity.Body.PrimaryHand.MaybeWeapon;
		if (base.CurrentEntity.Commands.Current is UnitUseAbility unitUseAbility)
		{
			itemEntityWeapon = unitUseAbility.Ability.Weapon;
		}
		return itemEntityWeapon?.Blueprint.WarhammerMaxDamage ?? 0;
	}
}
