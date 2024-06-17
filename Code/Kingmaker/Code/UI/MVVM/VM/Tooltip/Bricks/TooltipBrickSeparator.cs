using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickSeparator : ITooltipBrick
{
	private readonly TooltipBrickElementType m_Type;

	public TooltipBrickSeparator(TooltipBrickElementType type = TooltipBrickElementType.Big)
	{
		m_Type = type;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickSeparatorVM(m_Type);
	}
}
