using Kingmaker.UnitLogic.Buffs;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickBuff : ITooltipBrick
{
	public readonly Buff Buff;

	public TooltipBrickBuff(Buff buff)
	{
		Buff = buff;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickBuffVM(Buff);
	}
}
