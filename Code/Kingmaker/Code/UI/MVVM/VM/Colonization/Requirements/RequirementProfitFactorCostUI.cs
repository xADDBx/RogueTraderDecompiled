using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.UI.MVVM.VM.Colonization.Requirements;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Requirements;

public class RequirementProfitFactorCostUI : RequirementUI<RequirementProfitFactorCost>
{
	public override string Name => UIStrings.Instance.ProfitFactorTexts.Title;

	public override string Description => string.Format(UIStrings.Instance.ColonyProjectsRequirements.RequirementProfitFactorCost, string.Empty);

	public override Sprite Icon => BlueprintRoot.Instance.UIConfig.UIIcons.ProfitFactor;

	public override string NameForAcronym => null;

	public override string CountText => base.Requirement.ProfitFactorCost.ToString();

	public RequirementProfitFactorCostUI(RequirementProfitFactorCost requirement)
		: base(requirement)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		return new TooltipTemplateSimple(UIStrings.Instance.ProfitFactorTexts.Title.Text, UIStrings.Instance.ProfitFactorTexts.Description.Text);
	}
}
