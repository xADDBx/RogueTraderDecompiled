using System.Collections.Generic;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickPrerequisite : ITooltipBrick
{
	private readonly List<PrerequisiteEntryVM> m_PrerequisiteEntries;

	public TooltipBrickPrerequisite(List<PrerequisiteEntryVM> prerequisiteEntries)
	{
		m_PrerequisiteEntries = prerequisiteEntries;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickPrerequisiteVM(m_PrerequisiteEntries);
	}
}
