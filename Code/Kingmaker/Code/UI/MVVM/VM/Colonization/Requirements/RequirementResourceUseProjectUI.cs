using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.UI.MVVM.VM.Colonization.Requirements;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Requirements;

public class RequirementResourceUseProjectUI : RequirementUI<RequirementResourceUseProject>
{
	public override string Name => string.Empty;

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

	public override Sprite Icon => base.Requirement.ResourceBlueprint?.Icon;

	public override string NameForAcronym => null;

	public override string CountText => base.Requirement.Count.ToString();

	public RequirementResourceUseProjectUI(RequirementResourceUseProject requirement)
		: base(requirement)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		return new TooltipTemplateSimple(base.Requirement.ResourceBlueprint.Name, base.Requirement.ResourceBlueprint.Description);
	}
}
