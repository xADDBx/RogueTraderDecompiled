using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateCombatLogMessage : TooltipTemplateSimple
{
	public IEnumerable<ITooltipBrick> ExtraTooltipBricks = Enumerable.Empty<ITooltipBrick>();

	public IEnumerable<ITooltipBrick> ExtraInfoBricks = Enumerable.Empty<ITooltipBrick>();

	public TooltipTemplateCombatLogMessage(string header, string description, float contentSpacing = 0f)
		: base(header, description)
	{
		ContentSpacing = contentSpacing;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(base.Header, TooltipTitleType.H4, TextAlignmentOptions.Center, TextAnchor.MiddleCenter, 4);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		if (type == TooltipTemplateType.Tooltip)
		{
			return ExtraTooltipBricks.Concat(base.GetBody(type));
		}
		return ExtraInfoBricks.Concat(base.GetBody(type));
	}
}
