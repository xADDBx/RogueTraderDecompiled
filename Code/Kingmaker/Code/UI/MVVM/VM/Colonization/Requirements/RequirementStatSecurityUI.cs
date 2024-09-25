using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.UI.MVVM.VM.Colonization.Requirements;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Requirements;

public class RequirementStatSecurityUI : RequirementUI<RequireStatSecurity>
{
	public override string Name => string.Empty;

	public override string Description => UIStrings.Instance.ColonyProjectsRequirements.RequireStatSecurity.Text;

	public override Sprite Icon => BlueprintRoot.Instance.UIConfig.UIIcons.Security;

	public override string NameForAcronym => null;

	public override string CountText => base.Requirement.MinSecurityValue.ToString(">=0;<#");

	public RequirementStatSecurityUI(RequireStatSecurity requirement)
		: base(requirement)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		UIColonizationTexts.ColonyStatsStrings statStrings = UIStrings.Instance.ColonizationTexts.GetStatStrings(1);
		return new TooltipTemplateSimple(statStrings.Name, statStrings.Description);
	}
}
