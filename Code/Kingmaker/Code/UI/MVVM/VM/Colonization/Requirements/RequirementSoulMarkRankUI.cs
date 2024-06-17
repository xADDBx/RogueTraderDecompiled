using Kingmaker.Blueprints.Root;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Colonization.Requirements;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Requirements;

public class RequirementSoulMarkRankUI : RequirementUI<RequirementSoulMarkRank>
{
	public override string Name => string.Empty;

	public override string Description => UIUtility.GetSoulMarkDirectionText(base.Requirement.SoulMarkRequirement.Direction).Text + ": " + UIUtility.GetSoulMarkRankText(base.Requirement.SoulMarkRequirement.Rank).Text;

	public override Sprite Icon => UIConfig.Instance.UIIcons.SoulMarkIcons.GetIconByDirection(base.Requirement.SoulMarkRequirement.Direction);

	public override string NameForAcronym => null;

	public override string CountText => null;

	public RequirementSoulMarkRankUI(RequirementSoulMarkRank requirement)
		: base(requirement)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		return new TooltipTemplateSoulMarkHeader(Game.Instance.Player.MainCharacterEntity, base.Requirement.SoulMarkRequirement.Direction);
	}
}
