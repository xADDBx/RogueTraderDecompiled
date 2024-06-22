using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Localization;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.MVVM.VM.Tooltip.Templates;

public class TooltipTemplateCareerProgressionDesc : TooltipBaseTemplate
{
	private readonly CareerPathVM m_CareerPath;

	private UITextCharSheet Strings => UIStrings.Instance.CharacterSheet;

	private bool IsShip => m_CareerPath.Unit.IsPlayerShip();

	public TooltipTemplateCareerProgressionDesc(CareerPathVM careerPath)
	{
		m_CareerPath = careerPath;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		LocalizedString localizedString = (IsShip ? Strings.ShipCareerUpgradeHeader : Strings.CareerUpgradeHeader);
		yield return new TooltipBrickTitle(localizedString.Text);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		if (m_CareerPath.CanCommit.Value)
		{
			yield return new TooltipBrickText(UIStrings.Instance.CharacterSheet.CareerUpgradedDescription);
			yield break;
		}
		LocalizedString localizedString = (IsShip ? Strings.ShipCareerUpgradeDescription : Strings.CareerUpgradeDescription);
		yield return new TooltipBrickText(localizedString);
	}
}
