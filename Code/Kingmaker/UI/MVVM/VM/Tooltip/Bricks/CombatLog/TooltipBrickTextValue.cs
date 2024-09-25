using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class TooltipBrickTextValue : ITooltipBrick
{
	private readonly string m_Text;

	private readonly string m_Value;

	private readonly int m_NestedLevel;

	private readonly bool m_IsResultValue;

	private readonly bool m_NeedShowLine;

	public TooltipBrickTextValue(string text, string value, int nestedLevel = 0, bool isResultValue = false, bool needShowLine = true)
	{
		m_Text = text;
		m_Value = value;
		m_NestedLevel = nestedLevel;
		m_IsResultValue = isResultValue;
		m_NeedShowLine = needShowLine;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickTextValueVM(m_Text, m_Value, m_NestedLevel, m_IsResultValue, m_NeedShowLine);
	}
}
