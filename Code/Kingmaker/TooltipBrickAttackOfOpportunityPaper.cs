using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker;

public class TooltipBrickAttackOfOpportunityPaper : ITooltipBrick
{
	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickAttackOfOpportunityPaperVM();
	}
}
