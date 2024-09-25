using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;

public class TooltipBrickIconTextVM : TooltipBaseBrickVM
{
	public readonly string Text;

	public readonly bool IsShowIcon;

	public TooltipBrickIconTextVM(string text, bool isShowIcon = true)
	{
		Text = text;
		IsShowIcon = isShowIcon;
	}
}
