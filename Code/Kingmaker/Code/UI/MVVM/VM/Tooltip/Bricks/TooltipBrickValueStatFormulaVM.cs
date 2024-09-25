using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickValueStatFormulaVM : TooltipBaseBrickVM
{
	public readonly string Value;

	public readonly string Symbol;

	public readonly string Name;

	public TooltipBrickValueStatFormulaVM(string value, string symbol, string name)
	{
		Value = value;
		Symbol = symbol;
		Name = name;
	}
}
