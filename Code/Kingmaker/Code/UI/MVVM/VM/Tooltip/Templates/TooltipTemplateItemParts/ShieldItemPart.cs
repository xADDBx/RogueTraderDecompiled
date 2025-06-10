using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Shields;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Items;
using Kingmaker.UI.Models.Tooltip;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates.TooltipTemplateItemParts;

public class ShieldItemPart : WeaponItemPart
{
	public ShieldItemPart(ItemEntity item, ItemTooltipData itemTooltipData, ItemTooltipData compareItemTooltipData = null, bool isScreenWindowTooltip = false)
		: base(item, itemTooltipData, compareItemTooltipData, isScreenWindowTooltip)
	{
	}

	public ShieldItemPart(BlueprintItem blueprintItem, ItemTooltipData itemTooltipData, ItemTooltipData compareItemTooltipData = null, bool isScreenWindowTooltip = false)
		: base(blueprintItem, itemTooltipData, compareItemTooltipData, isScreenWindowTooltip)
	{
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		string itemName = GetItemName();
		string text = ItemTooltipData.GetText(TooltipElement.Subname);
		ItemEntity item = Item;
		Sprite image = ((item != null) ? ObjectExtensions.Or(item.Icon, null) : null) ?? SimpleBlueprintExtendAsObject.Or(BlueprintItem, null)?.Icon;
		list.Add(new TooltipBrickEntityHeader(itemName, image, hasUpgrade: false, text));
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddStats(list);
		AddAbilities(list);
		AddDescription(list, type);
		if (type == TooltipTemplateType.Info)
		{
			AddRestrictions(list, type);
		}
		return list;
	}

	private void AddStats(List<ITooltipBrick> result)
	{
		if (Item?.Blueprint is BlueprintItemShield blueprintItemShield)
		{
			result.Add(new TooltipBrickIconStatValue(UIStrings.Instance.BlockStrings.BaseBlockChance, $"{blueprintItemShield.BlockChance}%", null, BlueprintRoot.Instance.UIConfig.UIIcons.TooltipInspectIcons.CoverMagnitude));
		}
	}
}
