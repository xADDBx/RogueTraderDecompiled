using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateRace : TooltipBaseTemplate
{
	private readonly string m_Name;

	private readonly string m_Desc;

	public TooltipTemplateRace(BlueprintRace race)
	{
		if (race != null)
		{
			m_Name = race.Name;
			m_Desc = race.Description;
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(m_Name);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		yield return new TooltipBrickText(m_Desc, TooltipTextType.Paragraph);
	}
}
