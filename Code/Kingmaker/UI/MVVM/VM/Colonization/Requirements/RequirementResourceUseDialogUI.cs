using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Globalmap.Colonization.Requirements;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Colonization.Requirements;

public class RequirementResourceUseDialogUI : RequirementUI<RequirementResourceUseDialog>
{
	public override string Name => base.Requirement.ResourceBlueprint.Name;

	public override string Description
	{
		get
		{
			if (base.Requirement.ResourceBlueprint != null)
			{
				return base.Requirement.ResourceBlueprint.Name;
			}
			return string.Empty;
		}
	}

	public override Sprite Icon => base.Requirement.ResourceBlueprint.Icon;

	public override string NameForAcronym => null;

	public override string CountText => base.Requirement.Count.ToString();

	public bool BaseResourceCheck => base.Requirement.BaseResourceCheck();

	public string ProfitFactorName => UIStrings.Instance.ProfitFactorTexts.Title;

	public string ProfitFactorDescription => UIStrings.Instance.ColonyProjectsRequirements.RequirementProfitFactorCost;

	public Sprite ProfitFactorIcon => BlueprintRoot.Instance.UIConfig.UIIcons.ProfitFactor;

	public RequirementResourceUseDialogUI(RequirementResourceUseDialog requirement)
		: base(requirement)
	{
	}
}
