using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public static class TooltipPrerequisitesUtils
{
	public static void AddPrerequisiteGroup(List<PrerequisiteEntryVM> allPrerequisites, List<ITooltipBrick> result, bool isOr = false, bool showAnd = false)
	{
		List<PrerequisiteEntryVM> list = allPrerequisites.Where((PrerequisiteEntryVM p) => !p.Inverted && !p.IsGroup).ToList();
		List<PrerequisiteEntryVM> list2 = allPrerequisites.Where((PrerequisiteEntryVM p) => p.Inverted && !p.IsGroup).ToList();
		List<PrerequisiteEntryVM> list3 = allPrerequisites.Where((PrerequisiteEntryVM p) => p.IsGroup).ToList();
		bool flag = !isOr && showAnd && list.Any((PrerequisiteEntryVM p) => !p.IsTitle) && (list2.Any() || list3.Any());
		if (isOr && list.Any())
		{
			AddOr(result);
		}
		result.Add(new TooltipBrickPrerequisite(list));
		if (flag)
		{
			AddAnd(result);
			result.Add(new TooltipBricksGroupStart());
		}
		if (list2.Any())
		{
			if (isOr)
			{
				AddOr(result);
			}
			result.Add(new TooltipBrickTitle(UIStrings.Instance.Tooltips.NoFeature, TooltipTitleType.H3));
			result.Add(new TooltipBrickPrerequisite(list2));
		}
		if (isOr && list3.Any())
		{
			AddOr(result);
		}
		for (int i = 0; i < list3.Count; i++)
		{
			if (isOr && i > 0)
			{
				AddOr(result);
			}
			PrerequisiteEntryVM prerequisiteEntryVM = list3[i];
			showAnd &= list.All((PrerequisiteEntryVM p) => p.IsTitle);
			AddPrerequisiteGroup(prerequisiteEntryVM.Prerequisites, result, prerequisiteEntryVM.IsOrComposition, showAnd);
		}
		if (!flag)
		{
			result.Add(new TooltipBricksGroupEnd());
		}
	}

	public static void AddOr(List<ITooltipBrick> result)
	{
		result.Add(new TooltipBrickItemHeader($"<size={UIConfig.Instance.SubTextPercentSize}%>{UIStrings.Instance.Tooltips.or.Text}</size>"));
	}

	public static void AddAnd(List<ITooltipBrick> result)
	{
		result.Add(new TooltipBrickItemHeader($"<size={UIConfig.Instance.SubTextPercentSize}%>{UIStrings.Instance.Tooltips.and.Text}</size>"));
	}
}
