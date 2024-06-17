using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickHintVM : TooltipBaseBrickVM
{
	public readonly string Text;

	public TooltipBrickHintVM(string text)
	{
		Text = text;
	}
}
