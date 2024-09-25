using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickValueStatFormula : ITooltipBrick
{
	private string m_Value;

	private string m_Symbol;

	private string m_Name;

	public TooltipBrickValueStatFormula(string value, string symbol, string name)
	{
		m_Value = value;
		m_Symbol = symbol;
		m_Name = name;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickValueStatFormulaVM(m_Value, m_Symbol, m_Name);
	}
}
