using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickGlobalMapPosition : ITooltipBrick
{
	public virtual TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickGlobalMapPositionVM();
	}
}
