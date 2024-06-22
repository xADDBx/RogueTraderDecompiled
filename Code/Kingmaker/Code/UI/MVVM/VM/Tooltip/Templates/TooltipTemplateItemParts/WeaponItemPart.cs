using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Items;
using Kingmaker.UI.Models.Tooltip;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates.TooltipTemplateItemParts;

public class WeaponItemPart : BaseItemPart
{
	public WeaponItemPart(ItemEntity item, ItemTooltipData itemTooltipData, ItemTooltipData compareItemTooltipData = null, bool isScreenWindowTooltip = false)
		: base(item, itemTooltipData, compareItemTooltipData, isScreenWindowTooltip)
	{
	}

	public WeaponItemPart(BlueprintItem blueprintItem, ItemTooltipData itemTooltipData, ItemTooltipData compareItemTooltipData = null, bool isScreenWindowTooltip = false)
		: base(blueprintItem, itemTooltipData, compareItemTooltipData, isScreenWindowTooltip)
	{
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		string itemName = GetItemName();
		string text = ItemTooltipData.GetText(TooltipElement.Subname);
		BlueprintItemWeapon blueprintItemWeapon = (BlueprintItemWeapon)BlueprintItem;
		string text2 = (blueprintItemWeapon.IsTwoHanded ? UIStrings.Instance.InventoryScreen.TwoHandWeapon.Text : UIStrings.Instance.InventoryScreen.OneHandWeapon.Text);
		string weaponRangeLabel = UIStrings.Instance.WeaponCategories.GetWeaponRangeLabel(blueprintItemWeapon.Range);
		string weaponHeavinessLabel = UIStrings.Instance.WeaponCategories.GetWeaponHeavinessLabel(blueprintItemWeapon.Heaviness);
		if (type == TooltipTemplateType.Tooltip)
		{
			AddRestrictions(list, type);
		}
		string leftLabel = text2 + "\n" + weaponRangeLabel + " | " + weaponHeavinessLabel;
		string text3 = ItemTooltipData.GetText(TooltipElement.WeaponFamily);
		ItemEntity item = Item;
		Sprite image = ((item != null) ? ObjectExtensions.Or(item.Icon, null) : null) ?? SimpleBlueprintExtendAsObject.Or(BlueprintItem, null)?.Icon;
		bool hasUpgrade = BlueprintItem.PrototypeLink is BlueprintItemWeapon blueprintItemWeapon2 && blueprintItemWeapon2.CanBeUsedInGame;
		list.Add(new TooltipBrickEntityHeader(itemName, image, hasUpgrade, text, leftLabel, text3));
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddDamage(list);
		AddWeaponStats(list);
		AddAbilities(list);
		AddItemStatBonuses(list);
		AddDescription(list, type);
		if (type == TooltipTemplateType.Info)
		{
			AddRestrictions(list, type);
		}
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetFooter(TooltipTemplateType type)
	{
		return base.GetFooter(type);
	}

	private void AddWeaponStats(List<ITooltipBrick> bricks)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddDodgePenetration(list);
		AddAdditionalHitChance(list);
		AddRateOfFire(list);
		AddRange(list);
		AddMaxAmmo(list);
		if (list.Any())
		{
			bricks.AddRange(list);
		}
	}

	private void AddDodgePenetration(List<ITooltipBrick> bricks)
	{
		TryAddIconStatValue(bricks, TooltipElement.DodgeReduction, null, TooltipBrickIconStatValueType.Positive, TooltipBrickIconStatValueType.Positive);
	}

	private void AddAdditionalHitChance(List<ITooltipBrick> bricks)
	{
		TryAddIconStatValue(bricks, TooltipElement.AdditionalHitChance, null, TooltipBrickIconStatValueType.Positive, TooltipBrickIconStatValueType.Positive);
	}

	private void AddRateOfFire(List<ITooltipBrick> bricks)
	{
		int num;
		Sprite sprite;
		if (Item is ItemEntityWeapon itemEntityWeapon)
		{
			num = ((!itemEntityWeapon.Blueprint.IsRanged) ? 1 : 0);
			if (num != 0)
			{
				sprite = BlueprintRoot.Instance.UIConfig.UIIcons.RateOfFireMelee;
				goto IL_004d;
			}
		}
		else
		{
			num = 0;
		}
		sprite = BlueprintRoot.Instance.UIConfig.UIIcons.Crit;
		goto IL_004d;
		IL_004d:
		Sprite icon = sprite;
		TooltipElement element = ((num != 0) ? TooltipElement.RateOfFireMelee : TooltipElement.RateOfFire);
		TryAddIconStatValue(bricks, element, icon);
	}

	private void AddRange(List<ITooltipBrick> bricks)
	{
		Sprite range = BlueprintRoot.Instance.UIConfig.UIIcons.Range;
		TryAddIconStatValue(bricks, TooltipElement.Range, range);
	}

	private void AddMaxAmmo(List<ITooltipBrick> bricks)
	{
		Sprite attack = BlueprintRoot.Instance.UIConfig.UIIcons.Attack;
		TryAddIconStatValue(bricks, TooltipElement.MaxAmmo, attack);
	}
}
