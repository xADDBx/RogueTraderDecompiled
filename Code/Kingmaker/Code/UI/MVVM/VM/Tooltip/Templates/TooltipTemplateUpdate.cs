using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateUpdate : TooltipTemplateSimple
{
	public TooltipTemplateUpdate(string header, string description)
		: base(header, description)
	{
	}

	public override IEnumerable<ITooltipBrick> GetFooter(TooltipTemplateType type)
	{
		yield return new TooltipBrickSeparator();
		yield return new TooltipBrickTimer(delegate
		{
			DateTime now = DateTime.Now;
			return $"{now.Hour:D2}:{now.Minute:D2}:{now.Second:D2}";
		});
	}
}
