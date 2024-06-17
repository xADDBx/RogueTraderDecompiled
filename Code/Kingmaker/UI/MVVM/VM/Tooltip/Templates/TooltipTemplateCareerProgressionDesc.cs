using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateCareerProgressionDesc : TooltipBaseTemplate
{
	private readonly CareerPathVM m_CareerPath;

	public TooltipTemplateCareerProgressionDesc(CareerPathVM careerPath)
	{
		m_CareerPath = careerPath;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(UIStrings.Instance.CharacterSheet.CareerUpgradeHeader);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		if (m_CareerPath.CanCommit.Value)
		{
			yield return new TooltipBrickText(UIStrings.Instance.CharacterSheet.CareerUpgradedDescription);
		}
		else
		{
			yield return new TooltipBrickText(UIStrings.Instance.CharacterSheet.CareerUpgradeDescription);
		}
	}
}
