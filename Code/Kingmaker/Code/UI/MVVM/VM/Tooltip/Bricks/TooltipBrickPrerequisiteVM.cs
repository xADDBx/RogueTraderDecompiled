using System.Collections.Generic;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickPrerequisiteVM : TooltipBaseBrickVM
{
	public readonly List<PrerequisiteEntryVM> PrerequisiteEntries;

	public TooltipBrickPrerequisiteVM(List<PrerequisiteEntryVM> prerequisiteEntries)
	{
		PrerequisiteEntries = prerequisiteEntries;
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
