using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickTextBackground : ITooltipBrick
{
	private readonly string m_Text;

	private readonly TooltipTextType m_Type;

	private readonly bool m_IsHeader;

	private readonly TooltipTextAlignment m_Alignment;

	private bool m_NeedChangeSize;

	private int m_TextSize;

	protected readonly bool m_IsGrayBackground;

	protected readonly bool m_IsGreenBackground;

	protected readonly bool m_IsRedBackground;

	public TooltipBrickTextBackground(string text, TooltipTextType type = TooltipTextType.Simple, bool isHeader = false, TooltipTextAlignment alignment = TooltipTextAlignment.Midl, bool needChangeSize = false, int textSize = 18, bool isGrayBackground = false, bool isGreenBackground = false, bool isRedBackground = false)
	{
		m_Text = text;
		m_Type = type;
		m_IsHeader = isHeader;
		m_Alignment = alignment;
		m_NeedChangeSize = needChangeSize;
		m_TextSize = textSize;
		m_IsGrayBackground = isGrayBackground;
		m_IsGreenBackground = isGreenBackground;
		m_IsRedBackground = isRedBackground;
	}

	public virtual TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickTextBackgroundVM(m_Text, m_Type, m_Alignment, m_IsHeader, m_NeedChangeSize, m_TextSize, m_IsGrayBackground, m_IsGreenBackground, m_IsRedBackground);
	}
}
