using System;
using Kingmaker.Code.UI.MVVM.VM.Colonization.Requirements;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.UI.MVVM.VM.Colonization.Requirements;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Colonization.Projects;

public class ColonyProjectsRequirementElementVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<Sprite> Icon = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<string> Description = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> CountText = new ReactiveProperty<string>();

	public readonly ReactiveProperty<bool> IsChecked = new ReactiveProperty<bool>();

	public Requirement Requirement { get; }

	private Colony Colony { get; }

	public RequirementElementVisualType VisualType { get; private set; }

	public ColonyProjectsRequirementElementVM(Requirement requirement, Colony colony, bool isJournal = false)
	{
		Requirement = requirement;
		Colony = colony;
		RequirementUI requirement2 = RequirementUIFactory.GetRequirement(requirement);
		SetVisualType(requirement2);
		Icon.Value = requirement2.Icon;
		Description.Value = ((requirement2 is RequirementProfitFactorCostUI && isJournal) ? requirement2.Name : requirement2.Description);
		CountText.Value = requirement2.CountText;
		IsChecked.Value = requirement.Check(colony);
	}

	protected override void DisposeImplementation()
	{
	}

	public void CheckRequirementValueStatus()
	{
		IsChecked.Value = Requirement.Check(Colony);
	}

	private void SetVisualType(RequirementUI requirementUI)
	{
		if (!(requirementUI is RequirementNotBuiltProjectInColonyUI))
		{
			if (!(requirementUI is RequirementBuiltProjectInColonyUI) && !(requirementUI is RequirementBuiltProjectGlobalUI) && !(requirementUI is RequirementBuiltAtLeastOneOfProjectsUI))
			{
				if (requirementUI is RequirementProfitFactorCostUI || requirementUI is RequirementProfitFactorMinimumUI || requirementUI is RequirementReputationUI || requirementUI is RequirementResourceHaveUI || requirementUI is RequirementResourceUseOrderUI || requirementUI is RequirementResourceUseProjectUI || requirementUI is RequirementSoulMarkRankUI || requirementUI is RequirementStatContentmentUI || requirementUI is RequirementStatEfficiencyUI || requirementUI is RequirementStatSecurityUI || requirementUI is RequirementCargoUI || requirementUI is RequirementReputationCostUI || requirementUI is RequirementResourceUseDialogUI || requirementUI is RequirementScrapUI)
				{
					VisualType = RequirementElementVisualType.Default;
				}
				else
				{
					VisualType = RequirementElementVisualType.Default;
				}
			}
			else
			{
				VisualType = RequirementElementVisualType.FullLengthDescBig;
			}
		}
		else
		{
			VisualType = RequirementElementVisualType.NoCheckmark;
		}
	}
}
