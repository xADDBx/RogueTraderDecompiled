using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickItemHeaderVM : TooltipBaseBrickVM
{
	public readonly string Text;

	public readonly ItemHeaderType Type;

	public TooltipBrickItemHeaderVM(string text, ItemHeaderType type)
	{
		Text = text;
		Type = type;
	}
}
