using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateSimple : TooltipBaseTemplate
{
	[CanBeNull]
	public string Header { get; }

	[CanBeNull]
	public string Description { get; }

	public TooltipTemplateSimple(string header, string description = null)
	{
		Header = header;
		Description = description;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(Header);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		if (Description != null)
		{
			yield return new TooltipBrickText(Description);
		}
	}
}
