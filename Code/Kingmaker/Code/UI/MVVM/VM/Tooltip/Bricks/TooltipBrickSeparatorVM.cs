using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickSeparatorVM : TooltipBaseBrickVM
{
	public readonly TooltipBrickElementType Type;

	public TooltipBrickSeparatorVM(TooltipBrickElementType type = TooltipBrickElementType.Big)
	{
		Type = type;
	}
}
