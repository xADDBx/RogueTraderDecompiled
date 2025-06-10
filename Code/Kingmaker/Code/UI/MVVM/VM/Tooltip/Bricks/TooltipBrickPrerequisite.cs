using System.Collections.Generic;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickPrerequisite : ITooltipBrick
{
	private readonly List<PrerequisiteEntryVM> m_PrerequisiteEntries;

	private readonly bool m_OneFromList;

	public TooltipBrickPrerequisite(List<PrerequisiteEntryVM> prerequisiteEntries, bool oneFromList)
	{
		m_PrerequisiteEntries = prerequisiteEntries;
		m_OneFromList = oneFromList;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickPrerequisiteVM(m_PrerequisiteEntries, m_OneFromList);
	}
}
