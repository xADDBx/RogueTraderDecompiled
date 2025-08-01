using System;
using System.Collections.Generic;
using System.Linq;
using Code.Enums;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.UI.Models.Tooltip;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.UnityExtensions;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.Blueprints.Slots;

namespace Kingmaker.UI.Common;

public static class ItemsFilter
{
	private static readonly Dictionary<ItemEntity, ItemTooltipData> s_ItemTooltipDataSet = new Dictionary<ItemEntity, ItemTooltipData>();

	private static readonly Dictionary<int, List<ItemEntity>> s_ItemsByStringHash = new Dictionary<int, List<ItemEntity>>();

	public static bool ShouldShowItem(ItemEntity item, ItemsFilterType filter)
	{
		return ShouldShowItem(item.Blueprint, filter);
	}

	public static bool ShouldShowItem(BlueprintItem blueprintItem, ItemsFilterType filter)
	{
		if (ShouldSkipItem())
		{
			return false;
		}
		ItemInventoryFiltersOverride component = blueprintItem.GetComponent<ItemInventoryFiltersOverride>();
		if (component != null)
		{
			if (filter != component.ItemFilterOverride)
			{
				return filter == ItemsFilterType.NoFilter;
			}
			return true;
		}
		switch (filter)
		{
		case ItemsFilterType.NoFilter:
			return true;
		case ItemsFilterType.Weapon:
			return blueprintItem is BlueprintItemWeapon;
		case ItemsFilterType.Armor:
			if (!(blueprintItem is BlueprintItemArmor))
			{
				return blueprintItem is BlueprintItemShield;
			}
			return true;
		case ItemsFilterType.Accessories:
			if (!(blueprintItem is BlueprintItemEquipmentNeck) && !(blueprintItem is BlueprintItemEquipmentRing) && !(blueprintItem is BlueprintItemEquipmentWrist) && !(blueprintItem is BlueprintItemEquipmentBelt) && !(blueprintItem is BlueprintItemEquipmentShoulders) && !(blueprintItem is BlueprintItemEquipmentGloves) && !(blueprintItem is BlueprintItemEquipmentFeet) && !(blueprintItem is BlueprintItemEquipmentHead) && !(blueprintItem is BlueprintItemEquipmentGlasses) && !(blueprintItem is BlueprintItemEquipmentShirt))
			{
				return blueprintItem is BlueprintItemEquipmentPetProtocol;
			}
			return true;
		case ItemsFilterType.Usable:
			if (blueprintItem is BlueprintItemEquipmentUsable || blueprintItem.Tag != 0)
			{
				return !(blueprintItem is BlueprintItemEquipmentPetProtocol);
			}
			return false;
		case ItemsFilterType.Notable:
			return UIUtilityItem.IsQuestItem(blueprintItem);
		case ItemsFilterType.NonUsable:
			if (!blueprintItem.IsNotable && !(blueprintItem is BlueprintItemEquipmentUsable) && !(blueprintItem is BlueprintItemWeapon) && !(blueprintItem is BlueprintItemArmor) && !(blueprintItem is BlueprintItemShield) && !(blueprintItem is BlueprintItemEquipmentNeck) && !(blueprintItem is BlueprintItemEquipmentRing) && !(blueprintItem is BlueprintItemEquipmentShoulders) && !(blueprintItem is BlueprintItemEquipmentGloves) && !(blueprintItem is BlueprintItemEquipmentFeet) && !(blueprintItem is BlueprintItemEquipmentHead) && !(blueprintItem is BlueprintItemEquipmentGlasses) && !(blueprintItem is BlueprintItemEquipmentWrist) && !(blueprintItem is BlueprintItemEquipmentShirt) && !(blueprintItem is BlueprintItemEquipmentBelt) && !(blueprintItem is BlueprintStarshipItem))
			{
				return blueprintItem.Tag == ItemTag.None;
			}
			return false;
		case ItemsFilterType.ShipNoFilter:
			return blueprintItem is BlueprintStarshipItem;
		case ItemsFilterType.ShipWeapon:
			return blueprintItem is BlueprintStarshipWeapon;
		case ItemsFilterType.ShipOther:
			if (!(blueprintItem is BlueprintItemVoidShieldGenerator) && !(blueprintItem is BlueprintItemLifeSustainer) && !(blueprintItem is BlueprintItemBridge) && !(blueprintItem is BlueprintItemArmorPlating) && !(blueprintItem is BlueprintItemAugerArray) && !(blueprintItem is BlueprintItemWarpDrives) && !(blueprintItem is BlueprintItemPlasmaDrives) && !(blueprintItem is BlueprintItemGellerFieldDevice))
			{
				return blueprintItem is BlueprintItemArsenal;
			}
			return true;
		case ItemsFilterType.PlasmaDrives:
			return blueprintItem is BlueprintItemPlasmaDrives;
		case ItemsFilterType.VoidShieldGenerator:
			return blueprintItem is BlueprintItemVoidShieldGenerator;
		case ItemsFilterType.ArmorPlating:
			return blueprintItem is BlueprintItemArmorPlating;
		case ItemsFilterType.AugerArray:
			return blueprintItem is BlueprintItemAugerArray;
		case ItemsFilterType.Dorsal:
			if (blueprintItem is BlueprintStarshipWeapon blueprintStarshipWeapon4)
			{
				return blueprintStarshipWeapon4.AllowedSlots.Contains(WeaponSlotType.Dorsal);
			}
			return false;
		case ItemsFilterType.Prow:
			if (blueprintItem is BlueprintStarshipWeapon blueprintStarshipWeapon3)
			{
				return blueprintStarshipWeapon3.AllowedSlots.Contains(WeaponSlotType.Prow);
			}
			return false;
		case ItemsFilterType.Port:
			if (blueprintItem is BlueprintStarshipWeapon blueprintStarshipWeapon2)
			{
				return blueprintStarshipWeapon2.AllowedSlots.Contains(WeaponSlotType.Port);
			}
			return false;
		case ItemsFilterType.Starboard:
			if (blueprintItem is BlueprintStarshipWeapon blueprintStarshipWeapon)
			{
				return blueprintStarshipWeapon.AllowedSlots.Contains(WeaponSlotType.Starboard);
			}
			return false;
		case ItemsFilterType.Arsenal:
			return blueprintItem is BlueprintItemArsenal;
		default:
			throw new ArgumentOutOfRangeException("filter", filter, null);
		}
		bool ShouldSkipItem()
		{
			return blueprintItem is BlueprintStarshipAmmo;
		}
	}

	public static bool IsMatchSearchRequest(ItemEntity item, string searchRequest)
	{
		if (string.IsNullOrEmpty(searchRequest))
		{
			return true;
		}
		string[] separator = new string[1] { ", " };
		string[] array = searchRequest.Split(separator, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length > 1)
		{
			return array.All((string searchWord) => IsMatchSearchRequest(item, searchWord));
		}
		if (item.Name.StringEntry(searchRequest, item))
		{
			return true;
		}
		foreach (ItemsFilterType value2 in Enum.GetValues(typeof(ItemsFilterType)))
		{
			if (LocalizedTexts.Instance.ItemsFilter.GetText(value2).Equals(searchRequest, StringComparison.InvariantCultureIgnoreCase))
			{
				return ShouldShowItem(item, value2);
			}
		}
		if (!s_ItemTooltipDataSet.TryGetValue(item, out var value))
		{
			value = TooltipsDataCache.Instance.GetItemTooltipData(item);
			s_ItemTooltipDataSet.Add(item, value);
		}
		foreach (KeyValuePair<TooltipElement, string> text in value.Texts)
		{
			switch (text.Key)
			{
			case TooltipElement.Name:
			case TooltipElement.Count:
			case TooltipElement.ItemType:
			case TooltipElement.Price:
			case TooltipElement.SellPrice:
			case TooltipElement.Wielder:
			case TooltipElement.WielderSlot:
			case TooltipElement.Damage:
			case TooltipElement.PhysicalDamage:
			case TooltipElement.EquipDamage:
			case TooltipElement.Charges:
			case TooltipElement.CasterLevel:
				continue;
			}
			if (text.Value.StringEntry(searchRequest, item))
			{
				return true;
			}
		}
		if (value.OtherDamage.Any((KeyValuePair<DamageType, string> e) => UIUtilityTexts.GetTextByKey(e.Key).StringEntry(searchRequest, item)))
		{
			return true;
		}
		return false;
	}

	private static bool StringEntry(this string targetString, string matchString, ItemEntity item)
	{
		if (targetString.IsNullOrEmpty())
		{
			return false;
		}
		int hashCode = matchString.GetHashCode();
		if (s_ItemsByStringHash.ContainsKey(hashCode) && s_ItemsByStringHash[hashCode].Contains(item))
		{
			return true;
		}
		bool num = targetString.IndexOf(matchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
		if (num)
		{
			if (s_ItemsByStringHash.TryGetValue(hashCode, out var value))
			{
				value.Add(item);
				return num;
			}
			s_ItemsByStringHash.Add(hashCode, new List<ItemEntity> { item });
		}
		return num;
	}

	public static List<ItemEntity> ItemSorter(List<ItemEntity> items, ItemsSorterType type, ItemsFilterType filter)
	{
		switch (type)
		{
		case ItemsSorterType.TypeUp:
			items.Sort((ItemEntity a, ItemEntity b) => CompareByType(a, b, filter));
			break;
		case ItemsSorterType.TypeDown:
			items.Sort((ItemEntity a, ItemEntity b) => CompareByType(a, b, filter));
			items.Reverse();
			break;
		case ItemsSorterType.CharacteristicsUp:
			items.Sort((ItemEntity a, ItemEntity b) => CompareByCharacteristic(a, b, filter));
			break;
		case ItemsSorterType.CharacteristicsDown:
			items.Sort((ItemEntity a, ItemEntity b) => CompareByCharacteristic(a, b, filter));
			items.Reverse();
			break;
		case ItemsSorterType.DateUp:
			items.Sort((ItemEntity a, ItemEntity b) => CompareByDate(a, b, filter));
			break;
		case ItemsSorterType.DateDown:
			items.Sort((ItemEntity a, ItemEntity b) => CompareByDate(a, b, filter));
			items.Reverse();
			break;
		case ItemsSorterType.NameUp:
			items.Sort((ItemEntity a, ItemEntity b) => CompareByName(a, b, filter));
			break;
		case ItemsSorterType.NameDown:
			items.Sort((ItemEntity a, ItemEntity b) => CompareByName(a, b, filter));
			items.Reverse();
			break;
		}
		return items;
	}

	private static int CompareByDate(ItemEntity a, ItemEntity b, ItemsFilterType filter)
	{
		int num = a.Time.CompareTo(b.Time);
		if (num == 0)
		{
			int num2 = GetItemType(a, filter).CompareTo(GetItemType(b, filter));
			if (num2 != 0)
			{
				return num2;
			}
			return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
		}
		return num;
	}

	private static int CompareByType(ItemEntity a, ItemEntity b, ItemsFilterType filter)
	{
		int typeCompareValue = TypeSorter.GetTypeCompareValue(a, filter);
		int typeCompareValue2 = TypeSorter.GetTypeCompareValue(b, filter);
		if (typeCompareValue == typeCompareValue2)
		{
			return 0;
		}
		if (typeCompareValue >= typeCompareValue2)
		{
			return 1;
		}
		return -1;
	}

	private static int CompareByCharacteristic(ItemEntity a, ItemEntity b, ItemsFilterType filter)
	{
		int defaultTypeCompareValue = CharacteristicSorter.GetDefaultTypeCompareValue(a, filter);
		int defaultTypeCompareValue2 = CharacteristicSorter.GetDefaultTypeCompareValue(b, filter);
		if (defaultTypeCompareValue == defaultTypeCompareValue2)
		{
			return 0;
		}
		if (defaultTypeCompareValue >= defaultTypeCompareValue2)
		{
			return 1;
		}
		return -1;
	}

	private static int CompareByName(ItemEntity a, ItemEntity b, ItemsFilterType filter)
	{
		int num = GetItemType(a, filter).CompareTo(ItemsItemType.Other);
		int num2 = GetItemType(b, filter).CompareTo(ItemsItemType.Other);
		int result = GetItemType(a, filter).CompareTo(GetItemType(b, filter));
		int result2 = string.Compare(a.Name.TrimStart('['), b.Name.TrimStart('['), StringComparison.Ordinal);
		if (num == -1 && num2 == -1)
		{
			return result2;
		}
		return result;
	}

	public static ItemsItemType GetItemType(ItemEntity item, ItemsFilterType filter = ItemsFilterType.NoFilter)
	{
		return GetItemType(item.Blueprint, filter);
	}

	public static ItemsItemType GetItemType(BlueprintItem blueprintItem, ItemsFilterType filter = ItemsFilterType.NoFilter)
	{
		if (!ShouldShowItem(blueprintItem, filter))
		{
			return ItemsItemType.Other;
		}
		if (!(blueprintItem is BlueprintItemWeapon))
		{
			if (!(blueprintItem is BlueprintItemShield))
			{
				if (!(blueprintItem is BlueprintItemArmor))
				{
					if (!(blueprintItem is BlueprintItemEquipmentShirt))
					{
						if (!(blueprintItem is BlueprintItemEquipmentRing))
						{
							if (!(blueprintItem is BlueprintItemEquipmentBelt))
							{
								if (!(blueprintItem is BlueprintItemEquipmentFeet))
								{
									if (!(blueprintItem is BlueprintItemEquipmentGloves))
									{
										if (!(blueprintItem is BlueprintItemEquipmentHead))
										{
											if (!(blueprintItem is BlueprintItemEquipmentGlasses))
											{
												if (!(blueprintItem is BlueprintItemEquipmentNeck))
												{
													if (!(blueprintItem is BlueprintItemEquipmentShoulders))
													{
														if (!(blueprintItem is BlueprintItemEquipmentWrist))
														{
															if (!(blueprintItem is BlueprintItemEquipmentUsable))
															{
																if (!(blueprintItem is BlueprintItemVoidShieldGenerator))
																{
																	if (!(blueprintItem is BlueprintItemPlasmaDrives))
																	{
																		if (!(blueprintItem is BlueprintItemArmorPlating))
																		{
																			if (!(blueprintItem is BlueprintItemArsenal))
																			{
																				if (!(blueprintItem is BlueprintItemAugerArray))
																				{
																					if (!(blueprintItem is BlueprintStarshipWeapon))
																					{
																						if (blueprintItem is BlueprintItemEquipmentPetProtocol blueprintItemEquipmentPetProtocol)
																						{
																							return blueprintItemEquipmentPetProtocol.PetType switch
																							{
																								PetType.Mastiff => ItemsItemType.PetProtocolMastiff, 
																								PetType.Raven => ItemsItemType.PetProtocolRaven, 
																								PetType.Eagle => ItemsItemType.PetProtocolEagle, 
																								PetType.ServoskullSwarm => ItemsItemType.PetProtocolSkulls, 
																								_ => ItemsItemType.PetProtocol, 
																							};
																						}
																						return ItemsItemType.NonUsable;
																					}
																					return ItemsItemType.StarshipWeapon;
																				}
																				return ItemsItemType.StarshipAugerArray;
																			}
																			return ItemsItemType.StarshipArsenal;
																		}
																		return ItemsItemType.StarshipArmorPlating;
																	}
																	return ItemsItemType.StarshipPlasmaDrives;
																}
																return ItemsItemType.StarshipVoidShieldGenerator;
															}
															return ItemsItemType.Usable;
														}
														return ItemsItemType.Wrist;
													}
													return ItemsItemType.Shoulders;
												}
												return ItemsItemType.Neck;
											}
											return ItemsItemType.Glasses;
										}
										return ItemsItemType.Head;
									}
									return ItemsItemType.Gloves;
								}
								return ItemsItemType.Feet;
							}
							return ItemsItemType.Belt;
						}
						return ItemsItemType.Ring;
					}
					return ItemsItemType.Shirt;
				}
				return ItemsItemType.Armor;
			}
			return ItemsItemType.Shield;
		}
		return ItemsItemType.Weapon;
	}
}
