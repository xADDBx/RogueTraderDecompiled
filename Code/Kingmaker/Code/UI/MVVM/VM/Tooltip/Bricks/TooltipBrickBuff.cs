using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Other;
using Kingmaker.UnitLogic.Buffs;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickBuff : ITooltipBrick
{
	public readonly Buff Buff;

	public readonly BuffUIGroup Group;

	public readonly List<Buff> AdditionalSources;

	public TooltipBrickBuff(Buff buff, BuffUIGroup group)
	{
		Buff = buff;
		Group = group;
	}

	public TooltipBrickBuff(Buff buff, BuffUIGroup group, List<Buff> additionalSources)
	{
		Buff = buff;
		Group = group;
		AdditionalSources = additionalSources;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickBuffVM(Buff, AdditionalSources);
	}
}
