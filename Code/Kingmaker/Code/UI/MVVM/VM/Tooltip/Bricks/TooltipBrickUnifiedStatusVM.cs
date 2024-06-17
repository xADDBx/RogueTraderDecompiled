using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickUnifiedStatusVM : TooltipBaseBrickVM
{
	public readonly UnifiedStatus Status;

	public readonly string Text;

	public TooltipBrickUnifiedStatusVM(UnifiedStatus status, string text)
	{
		Status = status;
		Text = text;
	}
}
