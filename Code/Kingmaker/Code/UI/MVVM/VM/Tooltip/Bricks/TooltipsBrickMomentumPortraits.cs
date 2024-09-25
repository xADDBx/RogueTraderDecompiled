using System.Collections.Generic;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipsBrickMomentumPortraits : ITooltipBrick
{
	private readonly List<TooltipBrickMomentumPortrait> m_MomentumPortraits;

	public TooltipsBrickMomentumPortraits(List<TooltipBrickMomentumPortrait> momentumPortraits)
	{
		m_MomentumPortraits = momentumPortraits;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBricksMomentumPortraitsVM(m_MomentumPortraits);
	}
}
