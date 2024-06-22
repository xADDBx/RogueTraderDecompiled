using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickText : ITooltipBrick
{
	private readonly string m_Text;

	private readonly TooltipTextType m_Type;

	private readonly bool m_IsHeader;

	private readonly TooltipTextAlignment m_Alignment;

	private readonly bool m_NeedChangeSize;

	private readonly int m_TextSize;

	public TooltipBrickText(string text, TooltipTextType type = TooltipTextType.Simple, bool isHeader = false, TooltipTextAlignment alignment = TooltipTextAlignment.Midl, bool needChangeSize = false, int textSize = 18)
	{
		m_Text = text;
		m_Type = type;
		m_IsHeader = isHeader;
		m_Alignment = alignment;
		m_NeedChangeSize = needChangeSize;
		m_TextSize = textSize;
	}

	public virtual TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickTextVM(m_Text, m_Type, m_Alignment, m_IsHeader, m_NeedChangeSize, m_TextSize);
	}
}
