using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("709bbf9dd09c0fc44a3f8586c5728ba6")]
public class WeaponMaxDistanceGetter : UnitPropertyGetter
{
	[SerializeField]
	private bool SecondWeapon;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (!SecondWeapon)
		{
			return "First Weapon of " + FormulaTargetScope.Current + " Max Distance";
		}
		return "Second Weapon of " + FormulaTargetScope.Current + " Max Distance";
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
			return maybeWeapon.Blueprint.WarhammerMaxDistance;
		}
		return maybeWeapon2.Blueprint.WarhammerMaxDistance;
	}
}
