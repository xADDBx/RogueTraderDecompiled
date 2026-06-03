using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items.Slots;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateAugmentationsInspect : TooltipTemplateUnitInspect
{
	public TooltipTemplateAugmentationsInspect(IReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit.Value)
	{
		m_UnitReactiveProperty = unit;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		return new List<ITooltipBrick>
		{
			new TooltipBrickTitle(UIStrings.Instance.UIAugmentations.UnitInspectAugmentationsHeader, TooltipTitleType.H2)
		};
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> result = new List<ITooltipBrick>();
		if (m_Unit == null)
		{
			return result;
		}
		if (!RootUIContext.Instance.IsAugmentationsShown)
		{
			return result;
		}
		UpdateAugmentations(m_Unit, result);
		return result;
	}

	private void UpdateAugmentations(BaseUnitEntity unit, List<ITooltipBrick> result)
	{
		foreach (KeyValuePair<BlueprintAugmentSlot, AugmentSlot> slot in unit.Body.Augments.Slots)
		{
			AugmentSlot value = slot.Value;
			if (value.HasItem)
			{
				ItemTooltipData itemTooltipData = UIUtilityItem.GetItemTooltipData(value.Item);
				BlueprintItemAugment itemBlueprint = value.ItemBlueprint;
				string text = itemTooltipData.GetText(TooltipElement.Name);
				string augmentationsSlotTitle = GetAugmentationsSlotTitle(value);
				Sprite image = ((itemBlueprint.Icon != null) ? itemBlueprint.Icon : SimpleBlueprintExtendAsObject.Or(value.ItemBlueprint, null)?.Icon);
				TooltipTemplateItem augmentTooltip = new TooltipTemplateItem(value.Item);
				result.Add(new TooltipBrickEntityHeader(text, image, hasUpgrade: false, augmentationsSlotTitle, null, null, null, isAugment: false, augmentTooltip));
			}
		}
	}

	protected string GetAugmentationsSlotTitle(AugmentSlot augmentSlot)
	{
		AugmentationsEnum augmentationsEnum;
		switch (augmentSlot.Blueprint.name)
		{
		case "AugmentSlot_Systems":
			augmentationsEnum = AugmentationsEnum.NervousSystem;
			break;
		case "AugmentSlot_Eye":
			augmentationsEnum = AugmentationsEnum.PreceptionSystem;
			break;
		case "AugmentSlot_Arm_Left":
			augmentationsEnum = AugmentationsEnum.LeftHand;
			break;
		case "AugmentSlot_Arm_Right":
			augmentationsEnum = AugmentationsEnum.RightHand;
			break;
		case "AugmentSlot_Torso":
			augmentationsEnum = AugmentationsEnum.InternalSystems;
			break;
		case "AugmentSlot_Legs":
			augmentationsEnum = AugmentationsEnum.Legs;
			break;
		case "AugmentSlot_Manipulus1":
		case "AugmentSlot_Pascal1":
			augmentationsEnum = AugmentationsEnum.Mech1;
			break;
		case "AugmentSlot_Manipulus2":
		case "AugmentSlot_Pascal2":
			augmentationsEnum = AugmentationsEnum.Mech2;
			break;
		case "AugmentSlot_Manipulus3":
		case "AugmentSlot_Pascal3":
			augmentationsEnum = AugmentationsEnum.Mech3;
			break;
		case "AugmentSlot_ForgeWorld":
			augmentationsEnum = AugmentationsEnum.Forgeworld;
			break;
		default:
			augmentationsEnum = AugmentationsEnum.NervousSystem;
			break;
		}
		return augmentationsEnum switch
		{
			AugmentationsEnum.NervousSystem => UIStrings.Instance.UIAugmentations.SlotNervousSystem, 
			AugmentationsEnum.PreceptionSystem => UIStrings.Instance.UIAugmentations.SlotPreceptionSystem, 
			AugmentationsEnum.LeftHand => UIStrings.Instance.UIAugmentations.SlotLeftHand, 
			AugmentationsEnum.Legs => UIStrings.Instance.UIAugmentations.SlotLegs, 
			AugmentationsEnum.InternalSystems => UIStrings.Instance.UIAugmentations.SlotInternalSystems, 
			AugmentationsEnum.RightHand => UIStrings.Instance.UIAugmentations.SlotRightHand, 
			AugmentationsEnum.Mech1 => UIStrings.Instance.UIAugmentations.SlotMech1, 
			AugmentationsEnum.Mech2 => UIStrings.Instance.UIAugmentations.SlotMech2, 
			AugmentationsEnum.Mech3 => UIStrings.Instance.UIAugmentations.SlotMech3, 
			AugmentationsEnum.Forgeworld => UIStrings.Instance.UIAugmentations.SlotForgeWorld, 
			_ => string.Empty, 
		};
	}
}
