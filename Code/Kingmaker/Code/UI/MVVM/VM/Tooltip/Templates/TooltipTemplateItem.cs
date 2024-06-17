using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates.TooltipTemplateItemParts;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Tooltip;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateItem : TooltipBaseTemplate
{
	private readonly ItemEntity m_Item;

	private readonly ItemEntity m_BaseItem;

	private readonly BlueprintItem m_BlueprintItem;

	private readonly BlueprintItem m_BaseBlueprintItem;

	private readonly bool m_ForceUpdateCache;

	private readonly bool m_Replenishing;

	private ItemTooltipData m_ItemTooltipData;

	private BaseItemPart m_ItemPart;

	private bool m_Comparative;

	public ItemEntity Item => m_Item;

	public override TooltipBackground Background => GetBackgroundColor();

	public TooltipTemplateItem(ItemEntity item, ItemEntity baseItem = null, bool forceUpdateCache = false, bool replenishing = false)
	{
		if (item != null)
		{
			m_Item = item;
			m_BlueprintItem = item.Blueprint;
			m_BaseItem = baseItem;
			m_ForceUpdateCache = forceUpdateCache;
			m_Replenishing = replenishing;
		}
	}

	public TooltipTemplateItem(BlueprintItem blueprintItem, BlueprintItem baseBlueprintItem = null, bool forceUpdateCache = false)
	{
		if (blueprintItem != null)
		{
			m_BlueprintItem = blueprintItem;
			m_BaseBlueprintItem = baseBlueprintItem;
			m_ForceUpdateCache = forceUpdateCache;
		}
	}

	public override void Prepare(TooltipTemplateType type)
	{
		if (m_BlueprintItem != null)
		{
			m_Comparative = m_BaseItem != null || m_BaseBlueprintItem != null;
			m_ItemTooltipData = ((m_Item != null) ? TooltipsDataCache.Instance.GetItemTooltipData(m_Item, m_ForceUpdateCache, m_Replenishing) : TooltipsDataCache.Instance.GetItemTooltipData(m_BlueprintItem, m_ForceUpdateCache));
			ItemTooltipData itemTooltipData = ((m_BaseItem != null) ? TooltipsDataCache.Instance.GetItemTooltipData(m_BaseItem, m_ForceUpdateCache, m_Replenishing) : ((m_BaseBlueprintItem != null) ? TooltipsDataCache.Instance.GetItemTooltipData(m_BaseBlueprintItem, m_ForceUpdateCache) : null));
			m_ItemPart = ((m_Item != null) ? ((BaseItemPart)Activator.CreateInstance(GetItemPartTemplateType(), m_Item, m_ItemTooltipData, itemTooltipData)) : ((BaseItemPart)Activator.CreateInstance(GetItemPartTemplateType(), m_BlueprintItem, m_ItemTooltipData, itemTooltipData)));
		}
	}

	private Type GetItemPartTemplateType()
	{
		BlueprintItem blueprintItem = m_BlueprintItem;
		if (!(blueprintItem is BlueprintItemWeapon))
		{
			if (!(blueprintItem is BlueprintItemEquipmentUsable))
			{
				if (blueprintItem is BlueprintItemArmor)
				{
					return typeof(ArmorItemPart);
				}
				return typeof(BaseItemPart);
			}
			return typeof(UsableItemPart);
		}
		return typeof(WeaponItemPart);
	}

	private TooltipBackground GetBackgroundColor()
	{
		if (m_Comparative)
		{
			return TooltipBackground.Gray;
		}
		if (UIUtility.GetCurrentSelectedUnit() != null)
		{
			bool[] equipPosibility = UIUtilityItem.GetEquipPosibility(m_Item);
			if (!equipPosibility[0] && !equipPosibility[1])
			{
				return TooltipBackground.Red;
			}
			if (equipPosibility[1])
			{
				return TooltipBackground.Yellow;
			}
		}
		return TooltipBackground.White;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		if (m_ItemTooltipData == null)
		{
			yield break;
		}
		string canDoText = UIUtilityTexts.GetCanDoText(m_Item, type);
		if (!string.IsNullOrEmpty(canDoText))
		{
			yield return new TooltipBrickText(canDoText, TooltipTextType.Italic, isHeader: true);
		}
		foreach (ITooltipBrick item in m_ItemPart.GetHeader(type))
		{
			yield return item;
		}
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		if (m_ItemTooltipData == null)
		{
			yield break;
		}
		foreach (ITooltipBrick item in m_ItemPart.GetBody(type))
		{
			yield return item;
		}
	}

	public override IEnumerable<ITooltipBrick> GetFooter(TooltipTemplateType type)
	{
		if (m_ItemTooltipData == null)
		{
			yield break;
		}
		foreach (ITooltipBrick item in m_ItemPart.GetFooter(type))
		{
			yield return item;
		}
	}
}
