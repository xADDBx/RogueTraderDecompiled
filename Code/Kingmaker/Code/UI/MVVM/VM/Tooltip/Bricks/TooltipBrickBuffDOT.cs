using Kingmaker.UnitLogic.Buffs;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickBuffDOT : ITooltipBrick
{
	public readonly Buff Buff;

	public TooltipBrickBuffDOT(Buff buff)
	{
		Buff = buff;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickBuffDOTVM(Buff);
	}
}
