using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.UI.MVVM.VM.Colonization.Requirements;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Requirements;

public class RequirementNotBuiltProjectInColonyUI : RequirementUI<RequirementNotBuiltProjectInColony>
{
	public override string Name => string.Empty;

	public override string Description => GetDescription();

	public override Sprite Icon => base.Requirement.NotBuiltProject.Icon;

	public override string NameForAcronym => null;

	public override string CountText => null;

	public RequirementNotBuiltProjectInColonyUI(RequirementNotBuiltProjectInColony requirement)
		: base(requirement)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		return new TooltipTemplateSimple(base.Requirement.NotBuiltProject.Name, base.Requirement.NotBuiltProject.Description);
	}

	private string GetDescription()
	{
		if (base.Requirement.NotBuiltProject == null)
		{
			PFLog.UI.Error("RequirementNotBuiltProjectInColony.GetUIText - NotBuiltProject is null!");
			return string.Empty;
		}
		return string.Format(UIStrings.Instance.ColonyProjectsRequirements.RequirementNotBuiltProjectInColony, base.Requirement.NotBuiltProject.Name);
	}
}
