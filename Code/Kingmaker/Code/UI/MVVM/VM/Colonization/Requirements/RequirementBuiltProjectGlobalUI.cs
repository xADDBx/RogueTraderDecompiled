using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.UI.MVVM.VM.Colonization.Requirements;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Requirements;

public class RequirementBuiltProjectGlobalUI : RequirementUI<RequirementBuiltProjectGlobal>
{
	public override string Name => string.Empty;

	public override string Description => string.Format(UIStrings.Instance.ColonyProjectsRequirements.RequirementBuiltProjectGlobal, base.Requirement.BuiltProject.Name);

	public override Sprite Icon => base.Requirement.BuiltProject.Icon;

	public override string NameForAcronym => null;

	public override string CountText => null;

	public RequirementBuiltProjectGlobalUI(RequirementBuiltProjectGlobal requirement)
		: base(requirement)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		return new TooltipTemplateSimple(base.Requirement.BuiltProject.Name, base.Requirement.BuiltProject.Description);
	}
}
