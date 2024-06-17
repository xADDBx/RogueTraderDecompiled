using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("fa233c8140970be47945cefc36eb38ba")]
public class WeaponAmmoGetter : UnitPropertyGetter
{
	[SerializeField]
	private bool SecondWeapon;

	protected override string GetInnerCaption()
	{
		if (!SecondWeapon)
		{
			return "First Weapon current ammo";
		}
		return "Second Weapon current ammo";
	}

	protected override int GetBaseValue()
	{
		ItemEntityWeapon maybeWeapon = base.CurrentEntity.Body.PrimaryHand.MaybeWeapon;
		ItemEntityWeapon maybeWeapon2 = base.CurrentEntity.Body.SecondaryHand.MaybeWeapon;
		if (!SecondWeapon && maybeWeapon == null)
		{
			return 0;
		}
		if (SecondWeapon && maybeWeapon2 == null)
		{
			return 0;
		}
		if (!SecondWeapon)
		{
			return maybeWeapon.CurrentAmmo;
		}
		return maybeWeapon2.CurrentAmmo;
	}
}
