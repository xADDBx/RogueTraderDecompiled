using System;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UITextInventory
{
	[Header("Filters")]
	public LocalizedString FilterTextAll;

	public LocalizedString FilterTextWeapon;

	public LocalizedString FilterTextArmor;

	public LocalizedString FilterTextAcessories;

	public LocalizedString FilterTextUsable;

	public LocalizedString FilterTextNotable;

	public LocalizedString FilterTextShipItem;

	public LocalizedString FilterTextOther;

	[Header("Another")]
	public LocalizedString OneHandWeapon;

	public LocalizedString TwoHandWeapon;

	public LocalizedString RangedWeapon;

	public LocalizedString MeleeWeapon;

	public LocalizedString ShowUnavailableItems;

	[Header("Console")]
	public LocalizedString ChooseItem;

	public LocalizedString ToggleStats;

	public LocalizedString ChangeWeaponSet;

	public LocalizedString FavoriteCategory;

	public LocalizedString MainHand;

	public LocalizedString OffHand;
}
