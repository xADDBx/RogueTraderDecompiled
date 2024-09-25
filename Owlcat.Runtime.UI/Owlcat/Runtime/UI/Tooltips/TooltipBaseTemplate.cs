using System.Collections.Generic;

namespace Owlcat.Runtime.UI.Tooltips;

public abstract class TooltipBaseTemplate
{
	public float ContentSpacing = 5f;

	public virtual TooltipBackground Background => TooltipBackground.White;

	public virtual void Prepare(TooltipTemplateType type)
	{
	}

	public virtual IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield break;
	}

	public virtual IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		yield break;
	}

	public virtual IEnumerable<ITooltipBrick> GetFooter(TooltipTemplateType type)
	{
		yield break;
	}

	public virtual IEnumerable<ITooltipBrick> GetHint(TooltipTemplateType type)
	{
		yield break;
	}
}
