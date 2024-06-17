using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickSpaceVM : TooltipBaseBrickVM
{
	public readonly float? Height;

	public TooltipBrickSpaceVM(float? height)
	{
		Height = height;
	}
}
