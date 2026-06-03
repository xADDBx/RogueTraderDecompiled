using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates.TooltipTemplateItemParts;

public class AugmentItemPart : WeaponItemPart
{
	public AugmentItemPart(ItemEntity item, ItemTooltipData itemTooltipData, ItemTooltipData compareItemTooltipData = null, bool isScreenWindowTooltip = false)
		: base(item, itemTooltipData, compareItemTooltipData, isScreenWindowTooltip)
	{
	}

	public AugmentItemPart(BlueprintItem blueprintItem, ItemTooltipData itemTooltipData, ItemTooltipData compareItemTooltipData = null, bool isScreenWindowTooltip = false)
		: base(blueprintItem, itemTooltipData, compareItemTooltipData, isScreenWindowTooltip)
	{
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		string itemName = GetItemName();
		string text = ItemTooltipData.GetText(TooltipElement.Subname);
		ItemEntity item = Item;
		Sprite image = ((item != null) ? ObjectExtensions.Or(item.Icon, null) : null) ?? SimpleBlueprintExtendAsObject.Or(BlueprintItem, null)?.Icon ?? Game.Instance.BlueprintRoot.UIConfig.UIIcons.DefaultItemIcon;
		if (type == TooltipTemplateType.Tooltip)
		{
			AddRestrictions(list, type);
		}
		list.Add(new TooltipBrickEntityHeader(itemName, image, hasUpgrade: false, text, null, null, null, isAugment: true));
		return list;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddEffectsDescription(list);
		AddEffectsDescription(list, positive: false);
		if (Item?.Blueprint is BlueprintItemAugment blueprintItemAugment)
		{
			UIUtilityItem.AddOverchargeAbility(list, Item, blueprintItemAugment, IsScreenWindowTooltip);
		}
		list.Add(new TooltipBrickTitle(UIStrings.Instance.UIAugmentations.TooltipCanBeInstalledInMedicare, TooltipTitleType.H8, TextAlignmentOptions.Left, TextAnchor.MiddleLeft));
		list.Add(new TooltipBrickSeparator());
		if (type == TooltipTemplateType.Info)
		{
			AddRestrictions(list, type);
		}
		return list;
	}

	private void AddEffectsDescription(List<ITooltipBrick> result, bool positive = true)
	{
		LocalizedString localizedString = (positive ? UIStrings.Instance.UIAugmentations.TooltipAugmentationPositiveHeader : UIStrings.Instance.UIAugmentations.TooltipAugmentationNegativeHeader);
		BaseUnitEntity value = Game.Instance.SelectionCharacter.SelectedUnitInUI.Value;
		BlueprintItem blueprint = Item.Blueprint;
		List<AddFactToEquipmentWielder> list = ((blueprint != null) ? blueprint.GetComponents<AddFactToEquipmentWielder>().ToList() : null);
		if (list == null)
		{
			return;
		}
		AddFactToEquipmentWielder[] array = (positive ? list.Where(delegate(AddFactToEquipmentWielder f)
		{
			BlueprintUnitFact fact2 = f.Fact;
			return fact2 != null && fact2.NameForAcronym != null && !f.Fact.NameForAcronym.Contains("Negative");
		}).ToArray() : list.Where(delegate(AddFactToEquipmentWielder f)
		{
			BlueprintUnitFact fact = f.Fact;
			return fact != null && fact.NameForAcronym != null && f.Fact.NameForAcronym.Contains("Negative");
		}).ToArray());
		if (!array.Any())
		{
			return;
		}
		TooltipBrickTitle item = new TooltipBrickTitle(localizedString, TooltipTitleType.H7, TextAlignmentOptions.Left, TextAnchor.MiddleLeft);
		List<ITooltipBrick> list2 = new List<ITooltipBrick>();
		for (int i = 0; i < array.Length; i++)
		{
			string description = array[i].Fact.Description;
			if (description != null && !(description == ""))
			{
				description = UIUtilityTexts.UpdateDescriptionWithUIProperties(description, value, selectedUnitIsPreview: true);
				list2.Add(new TooltipBrickText(description, TooltipTextType.Paragraph, isHeader: false, TooltipTextAlignment.Left, needChangeSize: false, 18, value));
				if (i != array.Length - 1)
				{
					list2.Add(new TooltipBrickSpace());
				}
			}
		}
		if (list2.Any())
		{
			result.Add(item);
			result.AddRange(list2);
		}
	}
}
