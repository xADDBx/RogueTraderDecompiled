using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Controllers;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.UI.MVVM.VM.Colonization.Requirements;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Requirements;

public class RequirementReputationUI : RequirementUI<RequirementReputation>
{
	public override string Name => UIStrings.Instance.CharacterSheet.GetFactionLabel(base.Requirement.Reputation.Faction);

	public override string Description => GetDescription();

	public override Sprite Icon => UIConfig.Instance.UIIcons.GetFactionIcon(base.Requirement.Reputation.Faction);

	public override string NameForAcronym => null;

	public override string CountText => GetCountText();

	public RequirementReputationUI(RequirementReputation requirement)
		: base(requirement)
	{
	}

	private string GetDescription()
	{
		string factionLabel = UIStrings.Instance.CharacterSheet.GetFactionLabel(base.Requirement.Reputation.Faction);
		return string.Format(UIStrings.Instance.ColonyProjectsRequirements.RequirementReputation, factionLabel);
	}

	private string GetCountText()
	{
		return ReputationHelper.GetReputationPointsByLevel(base.Requirement.Reputation.Faction, base.Requirement.Reputation.MinLevelValue).ToString(">=0;<#");
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		return new TooltipTemplateSimple(UIStrings.Instance.CharacterSheet.GetFactionLabel(base.Requirement.Reputation.Faction), UIStrings.Instance.CharacterSheet.GetFactionDescription(base.Requirement.Reputation.Faction));
	}
}
