using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickCalculatedFormula : ITooltipBrick
{
	private readonly string m_Title;

	private readonly string m_Description;

	private readonly string m_Value;

	private readonly bool m_UseFlexibleValueWidth;

	public TooltipBrickCalculatedFormula(string title, string description, string value, bool useFlexibleValueWidth = false)
	{
		m_Title = title;
		m_Description = description;
		m_Value = value;
		m_UseFlexibleValueWidth = useFlexibleValueWidth;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickCalculatedFormulaVM(m_Title, m_Description, m_Value, m_UseFlexibleValueWidth);
	}
}
