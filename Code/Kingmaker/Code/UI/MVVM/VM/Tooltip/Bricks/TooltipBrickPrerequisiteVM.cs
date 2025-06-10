using System.Collections.Generic;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickPrerequisiteVM : TooltipBaseBrickVM
{
	public readonly List<PrerequisiteEntryVM> PrerequisiteEntries;

	public readonly bool OneFromList;

	public TooltipBrickPrerequisiteVM(List<PrerequisiteEntryVM> prerequisiteEntries, bool oneFromList)
	{
		PrerequisiteEntries = prerequisiteEntries;
		OneFromList = oneFromList;
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		foreach (PrerequisiteEntryVM prerequisiteEntry in PrerequisiteEntries)
		{
			prerequisiteEntry.Dispose();
		}
		PrerequisiteEntries.Clear();
	}
}
