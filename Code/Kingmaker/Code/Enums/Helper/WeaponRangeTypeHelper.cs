using System;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.RuleSystem.Enum;

namespace Kingmaker.Code.Enums.Helper;

public static class WeaponRangeTypeHelper
{
	public static bool IsSuitableWeapon(this WeaponRangeType type, BlueprintItemWeapon weapon)
	{
		return type.IsSuitableAttackType(weapon.AttackType);
	}

	public static bool IsSuitableWeapon(this WeaponRangeType type, ItemEntityWeapon weapon)
	{
		return type.IsSuitableAttackType(weapon.Blueprint.AttackType);
	}

	public static bool IsSuitableAttackType(this WeaponRangeType type, AttackType attackType)
	{
		return type switch
		{
			WeaponRangeType.Melee => attackType == AttackType.Melee, 
			WeaponRangeType.Ranged => attackType == AttackType.Ranged, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}
}
