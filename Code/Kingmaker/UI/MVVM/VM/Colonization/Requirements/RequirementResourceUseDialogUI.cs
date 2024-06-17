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

	public RequirementResourceUseDialogUI(RequirementResourceUseDialog requirement)
		: base(requirement)
	{
	}
}
