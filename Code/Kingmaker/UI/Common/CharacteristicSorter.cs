using System.Collections.Generic;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Items;

namespace Kingmaker.UI.Common;

public static class CharacteristicSorter
{
	private const int MaxSubcategoriesCount = 100000;

	private static readonly Dictionary<WarhammerArmorCategory, int> CategoryWeight = new Dictionary<WarhammerArmorCategory, int>
	{
		{
			WarhammerArmorCategory.Power,
			0
		},
		{
			WarhammerArmorCategory.Heavy,
			1
		},
		{
			WarhammerArmorCategory.Medium,
			0
		}
	};

	public static int GetDefaultTypeCompareValue(ItemEntity item, ItemsFilterType filter)
	{
		int num = 0;
		int num2 = 2;
		if (item.Blueprint is BlueprintItemArmor blueprintItemArmor)
		{
			if (!CategoryWeight.TryGetValue(blueprintItemArmor.Category, out var value))
			{
				value = 2;
			}
			num = blueprintItemArmor.DamageAbsorption * 10 + value;
			num2 = 1;
		}
		else if (item.Blueprint is BlueprintItemWeapon blueprintItemWeapon)
		{
			num = 100 * (blueprintItemWeapon.WarhammerDamage + blueprintItemWeapon.WarhammerMaxDamage) + blueprintItemWeapon.WarhammerPenetration;
			num2 = 0;
		}
		return num2 * 100000 + num;
	}
}
