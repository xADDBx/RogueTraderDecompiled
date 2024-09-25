using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.UI.MVVM.VM.Colonization.Requirements;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Requirements;

public class RequirementStatEfficiencyUI : RequirementUI<RequireStatEfficiency>
{
	public override string Name => string.Empty;

	public override string Description => UIStrings.Instance.ColonyProjectsRequirements.RequireStatEfficiency.Text;

	public override Sprite Icon => BlueprintRoot.Instance.UIConfig.UIIcons.Efficiency;

	public override string NameForAcronym => null;

	public override string CountText => base.Requirement.MinEfficiencyValue.ToString(">=0;<#");

	public RequirementStatEfficiencyUI(RequireStatEfficiency requirement)
		: base(requirement)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		UIColonizationTexts.ColonyStatsStrings statStrings = UIStrings.Instance.ColonizationTexts.GetStatStrings(0);
		return new TooltipTemplateSimple(statStrings.Name, statStrings.Description);
	}
}
