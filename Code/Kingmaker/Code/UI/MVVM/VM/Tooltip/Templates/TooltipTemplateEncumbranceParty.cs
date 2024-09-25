using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Localization;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateEncumbranceParty : TooltipTemplateEncumbrance
{
	public TooltipTemplateEncumbranceParty()
	{
		Capacity = EncumbranceHelper.GetPartyCarryingCapacity();
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		LocalizedString partyEncumbrance = UIStrings.Instance.Tooltips.PartyEncumbrance;
		yield return new TooltipBrickTitle(partyEncumbrance);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		list.Add(new TooltipBrickEncumbrance(Capacity));
		AddPenalty(list);
		list.Add(new TooltipBrickSeparator(TooltipBrickElementType.Small));
		list.Add(new TooltipBrickText(UIUtility.GetGlossaryEntryName("PEInfo")));
		return list;
	}

	protected override void AddPenalty(List<ITooltipBrick> bricks)
	{
		Encumbrance encumbrance = Capacity.GetEncumbrance();
		if (encumbrance == Encumbrance.Light)
		{
			bricks.Add(new TooltipBrickSeparator(TooltipBrickElementType.Small));
			TooltipTemplateEncumbrance.AddTitle(bricks, UIUtility.GetGlossaryEntryName("NoPenalty"));
			return;
		}
		TooltipTemplateEncumbrance.AddTitle(bricks, UIUtility.GetGlossaryEntryName("Penalty"));
		if (encumbrance == Encumbrance.Overload)
		{
			TooltipTemplateEncumbrance.AddIconValueSmall(bricks, UIUtility.GetGlossaryEntryName("UEBlockMap"), null);
		}
	}
}
