using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickTextBackgroundVM : TooltipBaseBrickVM
{
	public readonly string Text;

	public readonly TooltipTextType Type;

	public readonly bool IsHeader;

	public readonly TooltipTextAlignment Alignment;

	public bool NeedChangeSize;

	public int TextSize;

	public readonly bool IsGrayBackground;

	public readonly bool IsGreenBackground;

	public readonly bool IsRedBackground;

	public TooltipBrickTextBackgroundVM(string text, TooltipTextType type, TooltipTextAlignment alignment = TooltipTextAlignment.Midl, bool isHeader = false, bool needChangeSize = false, int textSize = 18, bool isGrayBackground = false, bool isGreenBackground = false, bool isRedBackground = false)
	{
		Text = text;
		Type = type;
		IsHeader = isHeader;
		Alignment = alignment;
		NeedChangeSize = needChangeSize;
		TextSize = textSize;
		IsGrayBackground = isGrayBackground;
		IsGreenBackground = isGreenBackground;
		IsRedBackground = isRedBackground;
	}
}
