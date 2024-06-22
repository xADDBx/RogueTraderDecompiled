using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("3670afde0e8b4c418aba24b22927d2d8")]
public class CheckAbilityWeaponIsOneHanded : PropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		ItemEntityWeapon abilityWeapon = this.GetAbilityWeapon();
		if (abilityWeapon == null || !abilityWeapon.HoldInTwoHands)
		{
			return 1;
		}
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability Weapon is One-Handed";
	}
}
