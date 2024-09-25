using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class TooltipBrickIconText : ITooltipBrick
{
	private readonly string m_Text;

	private readonly bool m_IsShowIcon;

	public TooltipBrickIconText(string text, bool isShowIcon = true)
	{
		m_Text = text;
		m_IsShowIcon = isShowIcon;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickIconTextVM(m_Text, m_IsShowIcon);
	}
}
