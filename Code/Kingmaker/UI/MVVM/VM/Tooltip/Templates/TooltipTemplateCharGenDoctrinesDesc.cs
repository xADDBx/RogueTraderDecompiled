using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateCharGenDoctrinesDesc : TooltipBaseTemplate
{
	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(UIStrings.Instance.Tooltips.DoctrinesHeader);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		Color32 tooltipValue = UIConfig.Instance.TooltipColors.TooltipValue;
		tooltipValue.a = 64;
		list.Add(new TooltipBricksGroupStart(hasBackground: true, new TooltipBricksGroupLayoutParams(), tooltipValue));
		list.Add(new TooltipBrickText(UIStrings.Instance.Tooltips.DoctrinesShortDesc));
		list.Add(new TooltipBricksGroupEnd());
		list.Add(new TooltipBrickText(UIStrings.Instance.Tooltips.DoctrinesDescription));
		return list;
	}
}
