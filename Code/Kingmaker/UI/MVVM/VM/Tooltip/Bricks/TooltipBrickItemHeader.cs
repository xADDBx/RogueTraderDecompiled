using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickItemHeader : ITooltipBrick
{
	private readonly string m_Text;

	private readonly ItemHeaderType m_Type;

	public TooltipBrickItemHeader(string text, ItemHeaderType type = ItemHeaderType.Default)
	{
		m_Text = text;
		m_Type = type;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickItemHeaderVM(m_Text, m_Type);
	}
}
