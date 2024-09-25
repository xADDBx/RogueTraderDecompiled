using Kingmaker.Code.UI.MVVM.VM.Colonization.Requirements;
using Kingmaker.Globalmap.Colonization.Requirements;

namespace Kingmaker.UI.MVVM.VM.Colonization.Requirements;

public static class RequirementUIFactory
{
	public static RequirementUI GetRequirement(Requirement requirement)
	{
		return TryCreateRequirementBuiltProjectGlobalUI(requirement) ?? TryCreateRequirementBuiltProjectInColonyUI(requirement) ?? TryCreateRequirementNotBuiltProjectInColonyUI(requirement) ?? TryCreateRequirementProfitFactorCostUI(requirement) ?? TryCreateRequirementProfitFactorMinimumUI(requirement) ?? TryCreateRequirementReputationUI(requirement) ?? TryCreateRequirementResourceHaveUI(requirement) ?? TryCreateRequirementResourceUseOrderUI(requirement) ?? TryCreateRequirementResourceUseProjectUI(requirement) ?? TryCreateRequirementStatContentmentUI(requirement) ?? TryCreateRequirementStatEfficiencyUI(requirement) ?? TryCreateRequirementStatSecurityUI(requirement) ?? TryCreateRequirementBuiltAtLeastOneOfProjectsUI(requirement) ?? TryCreateRequirementSoulMarkRankUI(requirement) ?? TryCreateRequirementReputationCostUI(requirement) ?? TryCreateRequirementCargoUI(requirement) ?? TryCreateRequirementScrapUI(requirement) ?? TryCreateRequirementResourceUseDialogUI(requirement) ?? new RequirementUI(requirement);
	}

	private static RequirementUI TryCreateRequirementBuiltProjectGlobalUI(Requirement requirement)
	{
		if (!(requirement is RequirementBuiltProjectGlobal requirement2))
		{
			return null;
		}
		return new RequirementBuiltProjectGlobalUI(requirement2);
	}

	private static RequirementUI TryCreateRequirementBuiltProjectInColonyUI(Requirement requirement)
	{
		if (!(requirement is RequirementBuiltProjectInColony requirement2))
		{
			return null;
		}
		return new RequirementBuiltProjectInColonyUI(requirement2);
	}

	private static RequirementUI TryCreateRequirementNotBuiltProjectInColonyUI(Requirement requirement)
	{
		if (!(requirement is RequirementNotBuiltProjectInColony requirement2))
		{
			return null;
		}
		return new RequirementNotBuiltProjectInColonyUI(requirement2);
	}

	private static RequirementUI TryCreateRequirementProfitFactorCostUI(Requirement requirement)
	{
		if (!(requirement is RequirementProfitFactorCost requirement2))
		{
			return null;
		}
		return new RequirementProfitFactorCostUI(requirement2);
	}

	private static RequirementUI TryCreateRequirementProfitFactorMinimumUI(Requirement requirement)
	{
		if (!(requirement is RequirementProfitFactorMinimum requirement2))
		{
			return null;
		}
		return new RequirementProfitFactorMinimumUI(requirement2);
	}

	private static RequirementUI TryCreateRequirementReputationUI(Requirement requirement)
	{
		if (!(requirement is RequirementReputation requirement2))
		{
			return null;
		}
		return new RequirementReputationUI(requirement2);
	}

	private static RequirementUI TryCreateRequirementResourceHaveUI(Requirement requirement)
	{
		if (!(requirement is RequirementResourceHave requirement2))
		{
			return null;
		}
		return new RequirementResourceHaveUI(requirement2);
	}

	private static RequirementUI TryCreateRequirementResourceUseOrderUI(Requirement requirement)
	{
		if (!(requirement is RequirementResourceUseOrder requirement2))
		{
			return null;
		}
		return new RequirementResourceUseOrderUI(requirement2);
	}

	private static RequirementUI TryCreateRequirementResourceUseProjectUI(Requirement requirement)
	{
		if (!(requirement is RequirementResourceUseProject requirement2))
		{
			return null;
		}
		return new RequirementResourceUseProjectUI(requirement2);
	}

	private static RequirementUI TryCreateRequirementStatContentmentUI(Requirement requirement)
	{
		if (!(requirement is RequirementStatContentment requirement2))
		{
			return null;
		}
		return new RequirementStatContentmentUI(requirement2);
	}

	private static RequirementUI TryCreateRequirementStatEfficiencyUI(Requirement requirement)
	{
		if (!(requirement is RequireStatEfficiency requirement2))
		{
			return null;
		}
		return new RequirementStatEfficiencyUI(requirement2);
	}

	private static RequirementUI TryCreateRequirementStatSecurityUI(Requirement requirement)
	{
		if (!(requirement is RequireStatSecurity requirement2))
		{
			return null;
		}
		return new RequirementStatSecurityUI(requirement2);
	}

	private static RequirementUI TryCreateRequirementBuiltAtLeastOneOfProjectsUI(Requirement requirement)
	{
		if (!(requirement is RequirementBuiltAtLeastOneOfProjects requirement2))
		{
			return null;
		}
		return new RequirementBuiltAtLeastOneOfProjectsUI(requirement2);
	}

	private static RequirementUI TryCreateRequirementSoulMarkRankUI(Requirement requirement)
	{
		if (!(requirement is RequirementSoulMarkRank requirement2))
		{
			return null;
		}
		return new RequirementSoulMarkRankUI(requirement2);
	}

	private static RequirementUI TryCreateRequirementReputationCostUI(Requirement requirement)
	{
		if (!(requirement is RequirementReputationCost requirement2))
		{
			return null;
		}
		return new RequirementReputationCostUI(requirement2);
	}

	private static RequirementUI TryCreateRequirementCargoUI(Requirement requirement)
	{
		if (!(requirement is RequirementCargo requirement2))
		{
			return null;
		}
		return new RequirementCargoUI(requirement2);
	}

	private static RequirementUI TryCreateRequirementScrapUI(Requirement requirement)
	{
		if (!(requirement is RequirementScrap requirement2))
		{
			return null;
		}
		return new RequirementScrapUI(requirement2);
	}

	private static RequirementUI TryCreateRequirementResourceUseDialogUI(Requirement requirement)
	{
		if (!(requirement is RequirementResourceUseDialog requirement2))
		{
			return null;
		}
		return new RequirementResourceUseDialogUI(requirement2);
	}
}
