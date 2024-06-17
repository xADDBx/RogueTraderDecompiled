using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.UI.MVVM.VM.Colonization.Requirements;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Requirements;

public class RequirementStatContentmentUI : RequirementUI<RequirementStatContentment>
{
	public override string Name => string.Empty;

	public override string Description => UIStrings.Instance.ColonyProjectsRequirements.RequirementStatContentment.Text;

	public override Sprite Icon => BlueprintRoot.Instance.UIConfig.UIIcons.Contentment;

	public override string NameForAcronym => null;

	public override string CountText => base.Requirement.MinContentmentValue.ToString(">=0;<#");

	public RequirementStatContentmentUI(RequirementStatContentment requirement)
		: base(requirement)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		UIColonizationTexts.ColonyStatsStrings statStrings = UIStrings.Instance.ColonizationTexts.GetStatStrings(2);
		return new TooltipTemplateSimple(statStrings.Name, statStrings.Description);
	}
}
