using System.Collections.Generic;
using Kingmaker.Blueprints.Items;
using Kingmaker.Items;
using Kingmaker.UI.Models.Tooltip;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates.TooltipTemplateItemParts;

public class UsableItemPart : BaseItemPart
{
	public UsableItemPart(ItemEntity item, ItemTooltipData itemTooltipData, ItemTooltipData compareItemTooltipData = null, bool isScreenWindowTooltip = false)
		: base(item, itemTooltipData, compareItemTooltipData, isScreenWindowTooltip)
	{
	}

	public UsableItemPart(BlueprintItem blueprintItem, ItemTooltipData itemTooltipData, ItemTooltipData compareItemTooltipData = null, bool isScreenWindowTooltip = false)
		: base(blueprintItem, itemTooltipData, compareItemTooltipData, isScreenWindowTooltip)
	{
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddDamage(list);
		AddAbilities(list);
		AddItemStatBonuses(list);
		AddDescription(list, type);
		if (type == TooltipTemplateType.Info)
		{
			AddRestrictions(list, type);
		}
		AddReplenishing(list, type);
		return list;
	}
}
