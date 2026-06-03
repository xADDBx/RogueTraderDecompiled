using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.UI.MVVM.VM.Colonization.Requirements;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Requirements;

public class RequirementCombativityCostUI : RequirementUI<RequirementCombativityCost>
{
	public override string Name => UIStrings.Instance.ProfitFactorTexts.CombativityTitle;

	public override string Description => string.Format(UIStrings.Instance.ColonyProjectsRequirements.RequirementCombativityCost, string.Empty);

	public override Sprite Icon => BlueprintRoot.Instance.UIConfig.UIIcons.Combativity;

	public override string NameForAcronym => null;

	public override string CountText => base.Requirement.CombativityCost.ToString();

	public RequirementCombativityCostUI(RequirementCombativityCost requirement)
		: base(requirement)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		return new TooltipTemplateSimple(UIStrings.Instance.ProfitFactorTexts.CombativityTitle.Text, UIStrings.Instance.ProfitFactorTexts.CombativityDescription.Text);
	}
}
