using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Items;
using Kingmaker.UI.Models.Tooltip;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates.TooltipTemplateItemParts;

public class ArmorItemPart : BaseItemPart
{
	public ArmorItemPart(ItemEntity item, ItemTooltipData itemTooltipData, ItemTooltipData compareItemTooltipData = null, bool isScreenWindowTooltip = false)
		: base(item, itemTooltipData, compareItemTooltipData, isScreenWindowTooltip)
	{
	}

	public ArmorItemPart(BlueprintItem blueprintItem, ItemTooltipData itemTooltipData, ItemTooltipData compareItemTooltipData = null, bool isScreenWindowTooltip = false)
		: base(blueprintItem, itemTooltipData, compareItemTooltipData, isScreenWindowTooltip)
	{
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		string itemName = GetItemName();
		if (type == TooltipTemplateType.Tooltip)
		{
			AddRestrictions(list, type);
		}
		string text = ItemTooltipData.GetText(TooltipElement.Subname);
		string text2 = ItemTooltipData.GetText(TooltipElement.FullArmorClass);
		ItemEntity item = Item;
		Sprite image = ((item != null) ? ObjectExtensions.Or(item.Icon, null) : null) ?? SimpleBlueprintExtendAsObject.Or(BlueprintItem, null)?.Icon;
		bool hasUpgrade = BlueprintItem.PrototypeLink is BlueprintItemArmor;
		list.Add(new TooltipBrickEntityHeader(itemName, image, hasUpgrade, text, text2));
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddArmorStats(list);
		AddItemStatBonuses(list);
		AddDescription(list, type);
		if (type == TooltipTemplateType.Info)
		{
			AddRestrictions(list, type);
		}
		return list;
	}

	protected void AddDeflectionAbsorption(List<ITooltipBrick> bricks)
	{
		string text = ItemTooltipData.GetText(TooltipElement.ArmorDeflection);
		string text2 = ItemTooltipData.GetText(TooltipElement.ArmorAbsorption);
		if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2))
		{
			ComparisonResult comparisonLeft = ComparisonResult.Equal;
			ComparisonResult comparisonRight = ComparisonResult.Equal;
			if (base.HasCompareItem)
			{
				CompareData compareData = ItemTooltipData.GetCompareData(TooltipElement.ArmorDeflection);
				CompareData compareData2 = CompareItemTooltipData.GetCompareData(TooltipElement.ArmorDeflection);
				comparisonLeft = CompareValues(compareData.Value, compareData2.Value);
				CompareData compareData3 = ItemTooltipData.GetCompareData(TooltipElement.ArmorAbsorption);
				CompareData compareData4 = CompareItemTooltipData.GetCompareData(TooltipElement.ArmorAbsorption);
				comparisonRight = CompareValues(compareData3.Value, compareData4.Value);
			}
			bool highlightLeft = false;
			bool highlightRight = false;
			if (BlueprintItem.PrototypeLink is BlueprintItemArmor blueprintItemArmor && BlueprintItem is BlueprintItemArmor blueprintItemArmor2)
			{
				highlightLeft = blueprintItemArmor2.DamageDeflection > blueprintItemArmor.DamageDeflection;
				highlightRight = blueprintItemArmor2.DamageAbsorption > blueprintItemArmor.DamageAbsorption;
			}
			Sprite damage = UIConfig.Instance.UIIcons.Damage;
			Sprite penetration = UIConfig.Instance.UIIcons.Penetration;
			bricks.Add(new TooltipBrickTwoColumnsStat(ItemTooltipData.GetLabel(TooltipElement.ArmorDeflection), ItemTooltipData.GetLabel(TooltipElement.ArmorAbsorption), text, text2, damage, penetration, comparisonLeft, comparisonRight, highlightLeft, highlightRight));
		}
	}

	private void AddArmorStats(List<ITooltipBrick> bricks)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddAbsorption(list);
		AddDeflection(list);
		AddDodgePenalty(list);
		AddArmourDodgeChanceDescription(list);
		AddArmorDamageReduceDescription(list);
		if (list.Any())
		{
			bricks.AddRange(list);
		}
	}

	private void AddAbsorption(List<ITooltipBrick> bricks)
	{
		Sprite armorAbsorption = UIConfig.Instance.UIIcons.TooltipIcons.ArmorAbsorption;
		TryAddIconStatValue(bricks, TooltipElement.ArmorAbsorption, armorAbsorption);
	}

	private void AddDodgePenalty(List<ITooltipBrick> bricks)
	{
		Sprite dodgePenalty = BlueprintRoot.Instance.UIConfig.UIIcons.TooltipIcons.DodgePenalty;
		TryAddIconStatValue(bricks, TooltipElement.ArmorDodgePenalty, dodgePenalty, TooltipBrickIconStatValueType.Negative, TooltipBrickIconStatValueType.Negative);
	}

	private void AddDeflection(List<ITooltipBrick> bricks)
	{
		if (ItemTooltipData.GetCompareData(TooltipElement.ArmorDeflection).Value > 0)
		{
			Sprite damageDeflection = BlueprintRoot.Instance.UIConfig.UIIcons.TooltipInspectIcons.DamageDeflection;
			TryAddIconStatValue(bricks, TooltipElement.ArmorDeflection, damageDeflection);
		}
	}

	private void AddArmourDodgeChanceDescription(List<ITooltipBrick> bricks)
	{
		Sprite dodge = BlueprintRoot.Instance.UIConfig.UIIcons.TooltipInspectIcons.Dodge;
		TryAddIconAndName(bricks, TooltipElement.ArmourDodgeChanceDescription, dodge);
	}

	private void AddArmorDamageReduceDescription(List<ITooltipBrick> bricks)
	{
		Sprite damageDeflection = BlueprintRoot.Instance.UIConfig.UIIcons.TooltipInspectIcons.DamageDeflection;
		TryAddIconAndName(bricks, TooltipElement.ArmorDamageReduceDescription, damageDeflection);
	}
}
