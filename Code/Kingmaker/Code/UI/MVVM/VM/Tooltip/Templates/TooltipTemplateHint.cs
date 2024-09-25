using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateHint : TooltipBaseTemplate
{
	public string Text { get; }

	public TooltipTemplateHint(string text)
	{
		Text = text;
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		yield return new TooltipBrickText(Text);
	}
}
