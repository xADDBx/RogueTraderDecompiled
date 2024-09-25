using Kingmaker.Code.UI.MVVM.VM.Other;
using Kingmaker.UnitLogic.Buffs;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickBuff : ITooltipBrick
{
	public readonly Buff Buff;

	public readonly BuffUIGroup Group;

	public TooltipBrickBuff(Buff buff, BuffUIGroup group)
	{
		Buff = buff;
		Group = group;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickBuffVM(Buff);
	}
}
