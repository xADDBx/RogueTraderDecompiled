using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickSeparatorVM : TooltipBaseBrickVM
{
	public readonly TooltipBrickElementType Type;

	public readonly bool IsAugmentHeader;

	public TooltipBrickSeparatorVM(TooltipBrickElementType type = TooltipBrickElementType.Big, bool isAugmentHeader = false)
	{
		Type = type;
		IsAugmentHeader = isAugmentHeader;
	}
}
