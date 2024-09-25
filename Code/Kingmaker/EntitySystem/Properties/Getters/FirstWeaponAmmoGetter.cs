using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Commands;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("645442d6cb1bd4d48aeee04e283d1ed1")]
public class FirstWeaponAmmoGetter : UnitPropertyGetter
{
	[SerializeField]
	private bool percent;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (!percent)
		{
			return "Ammo (of " + FormulaTargetScope.Current + ")";
		}
		return "Ammo percent (of " + FormulaTargetScope.Current + ")";
	}

	protected override int GetBaseValue()
	{
		ItemEntityWeapon itemEntityWeapon = base.CurrentEntity.Body.PrimaryHand.MaybeWeapon;
		if (base.CurrentEntity.Commands.Current is UnitUseAbility unitUseAbility)
		{
			itemEntityWeapon = unitUseAbility.Ability.Weapon;
		}
		if (itemEntityWeapon == null)
		{
			if (!percent)
			{
				return 1;
			}
			return 100;
		}
		float num = ((float?)base.CurrentEntity?.Commands?.Current?.Animation?.BurstCount) ?? 0f;
		int num2 = 0;
		if (num > 1f)
		{
			num2 = Mathf.FloorToInt(Math.Clamp(num - (((float?)base.CurrentEntity?.Commands?.Current?.Animation?.ActEventsCounter) ?? 0f), 0f, num));
		}
		if (!percent)
		{
			return Mathf.FloorToInt((float)(itemEntityWeapon.CurrentAmmo + num2) * 100f / (float)itemEntityWeapon.Blueprint.WarhammerMaxAmmo);
		}
		return itemEntityWeapon.CurrentAmmo + num2;
	}
}
