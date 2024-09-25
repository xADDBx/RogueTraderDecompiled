using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateColonyStatInfo : TooltipTemplateSimple
{
	public TooltipTemplateColonyStatInfo(string header, string description)
		: base(header, description)
	{
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(base.Header, TooltipTitleType.H2, TextAlignmentOptions.Left);
	}
}
