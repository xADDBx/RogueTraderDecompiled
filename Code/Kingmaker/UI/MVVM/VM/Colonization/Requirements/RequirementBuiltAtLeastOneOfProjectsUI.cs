using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Requirements;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Colonization.Requirements;

public class RequirementBuiltAtLeastOneOfProjectsUI : RequirementUI<RequirementBuiltAtLeastOneOfProjects>
{
	public override string Name => string.Empty;

	public override string Description => GetDescription();

	public override Sprite Icon => null;

	public override string NameForAcronym => null;

	public override string CountText => null;

	public RequirementBuiltAtLeastOneOfProjectsUI(RequirementBuiltAtLeastOneOfProjects requirement)
		: base(requirement)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		return new TooltipTemplateSimple(UIStrings.Instance.CommonTexts.Information, GetDescription());
	}

	private string GetDescription()
	{
		string text = "";
		for (int i = 0; i < base.Requirement.Projects.Count; i++)
		{
			if (i > 0)
			{
				text += ", ";
			}
			text += base.Requirement.Projects[i].Name;
		}
		return string.Format(UIStrings.Instance.ColonyProjectsRequirements.RequirementBuiltProjectOneOf, text);
	}
}
