using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBricksGroupEnd : ITooltipBrick
{
	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBricksGroupVM(TooltipBricksGroupType.End, hasBackground: true, null, null);
	}
}
