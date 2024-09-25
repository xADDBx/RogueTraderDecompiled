using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickAttributeVM : TooltipBaseBrickVM
{
	public readonly string Name;

	public readonly string Acronym;

	public readonly bool IsRecommended;

	public readonly StripeType StripeType;

	public readonly TooltipBaseTemplate Tooltip;

	public TooltipBrickAttributeVM(string name, string acronym, TooltipBaseTemplate tooltip, StripeType type, bool isRecommended)
	{
		Name = name;
		Acronym = acronym;
		StripeType = type;
		IsRecommended = isRecommended;
		Tooltip = tooltip;
	}
}
