using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Enums;
using Kingmaker.Items;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UI.Common;

public static class TypeSorter
{
	public static readonly List<WeaponFamily> MeleeWeaponSortingOrder = new List<WeaponFamily>
	{
		WeaponFamily.Chain,
		WeaponFamily.Exotic,
		WeaponFamily.Power,
		WeaponFamily.Primitive
	};

	public static readonly List<WeaponFamily> RangeWeaponSortingOrder = new List<WeaponFamily>
	{
		WeaponFamily.Bolt,
		WeaponFamily.Exotic,
		WeaponFamily.Flame,
		WeaponFamily.Laser,
		WeaponFamily.Melta,
		WeaponFamily.Plasma,
		WeaponFamily.Solid
	};

	private static readonly List<ItemsItemType> EquipSortingOrder = new List<ItemsItemType>
	{
		ItemsItemType.Head,
		ItemsItemType.Neck,
		ItemsItemType.Ring,
		ItemsItemType.Gloves,
		ItemsItemType.Feet,
		ItemsItemType.Shoulders
	};

	private static readonly List<ItemsItemType> ShipSortingOrder = new List<ItemsItemType>
	{
		ItemsItemType.StarshipVoidShieldGenerator,
		ItemsItemType.StarshipPlasmaDrives,
		ItemsItemType.StarshipArmorPlating,
		ItemsItemType.StarshipAugerArray,
		ItemsItemType.StarshipWeapon,
		ItemsItemType.StarshipArsenal
	};

	private static readonly List<ItemsItemOrigin> OriginSortingOrder = new List<ItemsItemOrigin>
	{
		ItemsItemOrigin.Textile,
		ItemsItemOrigin.Armours,
		ItemsItemOrigin.Chaos,
		ItemsItemOrigin.Xeno,
		ItemsItemOrigin.Torpedoes,
		ItemsItemOrigin.Fuel,
		ItemsItemOrigin.Holy,
		ItemsItemOrigin.Jewelry,
		ItemsItemOrigin.MeleeWeaponry,
		ItemsItemOrigin.Miscellaneous,
		ItemsItemOrigin.People,
		ItemsItemOrigin.Provisions,
		ItemsItemOrigin.RangedWeaponry,
		ItemsItemOrigin.ShipComponents,
		ItemsItemOrigin.SpaceAeldari,
		ItemsItemOrigin.SpaceChaos,
		ItemsItemOrigin.SpaceDrukhari,
		ItemsItemOrigin.SpaceNecrons,
		ItemsItemOrigin.SpacePirates,
		ItemsItemOrigin.Tech
	};

	private const int MaxSubcategoriesCount = 100;

	public static int GetTypeCompareValue(ItemEntity item, ItemsFilterType filter)
	{
		switch (filter)
		{
		case ItemsFilterType.Notable:
			if (!UIUtilityItem.IsEquipPossible(item))
			{
				return 1;
			}
			return 0;
		case ItemsFilterType.NonUsable:
		{
			int num = OriginSortingOrder.IndexOf(item.Blueprint.Origin);
			if (num < 0)
			{
				return OriginSortingOrder.Count;
			}
			return num;
		}
		default:
			return GetDefaultTypeCompareValue(item, filter);
		}
	}

	public static int GetDefaultTypeCompareValue(ItemEntity item, ItemsFilterType filter)
	{
		ItemsItemType itemType = ItemsFilter.GetItemType(item, filter);
		int num = 0;
		int num2;
		switch (itemType)
		{
		case ItemsItemType.Shield:
		case ItemsItemType.Armor:
			if (item.Blueprint is BlueprintItemArmor blueprintItemArmor)
			{
				num = (int)blueprintItemArmor.Category;
			}
			num2 = 1;
			break;
		case ItemsItemType.Weapon:
			if (item.Blueprint is BlueprintItemWeapon blueprintItemWeapon)
			{
				bool num3 = blueprintItemWeapon.Category == WeaponCategory.Melee;
				int num4 = (blueprintItemWeapon.IsTwoHanded ? 1 : 0);
				List<WeaponFamily> list = (num3 ? MeleeWeaponSortingOrder : RangeWeaponSortingOrder);
				int num5 = (num3 ? (num4 * (MeleeWeaponSortingOrder.Count + 1)) : (num4 * (RangeWeaponSortingOrder.Count + 1) + Enum.GetValues(typeof(WeaponHoldingType)).Length * (MeleeWeaponSortingOrder.Count + 1)));
				int num6 = list.IndexOf(blueprintItemWeapon.Family);
				num = ((num6 >= 0) ? (num6 + num5) : (list.Count + num5));
			}
			num2 = 0;
			break;
		case ItemsItemType.Ring:
		case ItemsItemType.Feet:
		case ItemsItemType.Gloves:
		case ItemsItemType.Head:
		case ItemsItemType.Neck:
		case ItemsItemType.Shoulders:
		{
			int num7 = EquipSortingOrder.IndexOf(itemType);
			num = ((num7 >= 0) ? num7 : EquipSortingOrder.Count);
			num2 = 2;
			break;
		}
		case ItemsItemType.Usable:
			if (item.Blueprint is BlueprintItemEquipmentUsable blueprintItemEquipmentUsable)
			{
				num = (int)blueprintItemEquipmentUsable.Tag;
			}
			num2 = 3;
			break;
		case ItemsItemType.StarshipWeapon:
		case ItemsItemType.StarshipPlasmaDrives:
		case ItemsItemType.StarshipVoidShieldGenerator:
		case ItemsItemType.StarshipAugerArray:
		case ItemsItemType.StarshipArmorPlating:
		case ItemsItemType.StarshipArsenal:
		{
			int num8 = ShipSortingOrder.IndexOf(itemType);
			if (num8 < 0)
			{
				num8 = ShipSortingOrder.Count + Enum.GetNames(typeof(StarshipWeaponType)).Length;
			}
			if (itemType == ItemsItemType.StarshipWeapon && item.Blueprint is BlueprintStarshipWeapon blueprintStarshipWeapon)
			{
				num8 = (int)(num8 + blueprintStarshipWeapon.WeaponType);
			}
			num = num8;
			num2 = 6;
			break;
		}
		default:
			num2 = ((item.Blueprint.IsNotable || item.Blueprint.Rarity == BlueprintItem.ItemRarity.Quest) ? 4 : 5);
			break;
		}
		return num2 * 100 + num;
	}
}
