using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class TooltipBrickTextValueVM : TooltipBaseBrickVM
{
	public readonly string Text;

	public readonly string Value;

	public readonly int NestedLevel;

	public readonly bool IsResultValue;

	public readonly bool NeedShowLine;

	public TooltipBrickTextValueVM(string text, string value, int nestedLevel, bool isResultValue, bool needShowLine = true)
	{
		Text = text;
		Value = value;
		NestedLevel = nestedLevel;
		IsResultValue = isResultValue;
		NeedShowLine = needShowLine;
	}
}
