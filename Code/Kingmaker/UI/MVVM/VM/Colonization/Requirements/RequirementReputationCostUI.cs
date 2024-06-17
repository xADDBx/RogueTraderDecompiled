using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Requirements;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Colonization.Requirements;

public class RequirementReputationCostUI : RequirementUI<RequirementReputationCost>
{
	public override string Name => UIStrings.Instance.CharacterSheet.GetFactionLabel(base.Requirement.Faction);

	public override string Description => GetDescription();

	public override Sprite Icon => BlueprintRoot.Instance.UIConfig.UIIcons.GetFactionIcon(base.Requirement.Faction);

	public override string NameForAcronym => null;

	public override string CountText => base.Requirement.Reputation + " " + UIStrings.Instance.Tooltips.ReputationPointsAbbreviation.Text;

	public RequirementReputationCostUI(RequirementReputationCost requirement)
		: base(requirement)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		return new TooltipTemplateSimple(UIStrings.Instance.CharacterSheet.GetFactionLabel(base.Requirement.Faction), UIStrings.Instance.CharacterSheet.GetFactionDescription(base.Requirement.Faction));
	}

	private string GetDescription()
	{
		string factionLabel = UIStrings.Instance.CharacterSheet.GetFactionLabel(base.Requirement.Faction);
		return string.Format(UIStrings.Instance.ColonyProjectsRequirements.RequirementReputation, factionLabel);
	}
}
