using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.UI.MVVM.VM.Colonization.Requirements;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Requirements;

public class RequirementResourceHaveUI : RequirementUI<RequirementResourceHave>
{
	public override string Name
	{
		get
		{
			if (base.Requirement.Resource != null)
			{
				return base.Requirement.Resource.Name;
			}
			return string.Empty;
		}
	}

	public override string Description
	{
		get
		{
			if (base.Requirement.Resource != null)
			{
				return base.Requirement.Resource.Name;
			}
			return string.Empty;
		}
	}

	public override Sprite Icon => base.Requirement.Resource?.Icon;

	public override string NameForAcronym => string.Empty;

	public override int Count => base.Requirement.Count;

	public override string CountText => base.Requirement.Count.ToString(">=0;<#");

	public RequirementResourceHaveUI(RequirementResourceHave requirement)
		: base(requirement)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		return new TooltipTemplateSimple(base.Requirement.Resource.Name, base.Requirement.Resource.Description);
	}
}
