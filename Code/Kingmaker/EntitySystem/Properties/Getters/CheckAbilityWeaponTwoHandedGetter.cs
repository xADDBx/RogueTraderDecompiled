using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("4a65f37aabb64aaa8d459f405fb5e369")]
public class CheckAbilityWeaponTwoHandedGetter : PropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		ItemEntityWeapon abilityWeapon = this.GetAbilityWeapon();
		if (abilityWeapon == null)
		{
			return 0;
		}
		if (!abilityWeapon.Blueprint.IsTwoHanded)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption()
	{
		return "Ability Weapon Family";
	}
}
