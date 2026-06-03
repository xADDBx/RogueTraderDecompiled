using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickSeparator : ITooltipBrick
{
	private readonly TooltipBrickElementType m_Type;

	private readonly bool m_IsAugmentHeader;

	public TooltipBrickSeparator(TooltipBrickElementType type = TooltipBrickElementType.Big, bool isAugmentHeader = false)
	{
		m_Type = type;
		m_IsAugmentHeader = isAugmentHeader;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickSeparatorVM(m_Type, m_IsAugmentHeader);
	}
}
