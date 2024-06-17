using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickTextVM : TooltipBaseBrickVM
{
	public readonly string Text;

	public readonly TooltipTextType Type;

	public readonly bool IsHeader;

	public readonly TooltipTextAlignment Alignment;

	public bool NeedChangeSize;

	public int TextSize;

	public TooltipBrickTextVM(string text, TooltipTextType type, TooltipTextAlignment alignment = TooltipTextAlignment.Midl, bool isHeader = false, bool needChangeSize = false, int textSize = 18)
	{
		Text = text;
		Type = type;
		IsHeader = isHeader;
		Alignment = alignment;
		NeedChangeSize = needChangeSize;
		TextSize = textSize;
	}
}
