using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickSpace : ITooltipBrick
{
	private readonly float? m_Height;

	public TooltipBrickSpace()
	{
	}

	public TooltipBrickSpace(float height)
	{
		m_Height = height;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickSpaceVM(m_Height);
	}
}
