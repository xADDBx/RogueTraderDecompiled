using System.Collections.Generic;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBricksMomentumPortraitsVM : TooltipBaseBrickVM
{
	public readonly List<TooltipBrickMomentumPortrait> MomentumPortraits;

	public TooltipBricksMomentumPortraitsVM(List<TooltipBrickMomentumPortrait> momentumPortraits)
	{
		MomentumPortraits = momentumPortraits;
	}
}
