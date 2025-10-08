using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Rewards;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Colonization.Rewards;

public class RewardNavigatorResourceUI : RewardUI<RewardNavigatorResource>
{
	public override Sprite Icon => UIConfig.Instance.UIIcons.TooltipInspectIcons.PsyRating;

	public override int Count => base.Reward.NavigatorResourceCount;

	public override string CountText => "+" + base.Reward.NavigatorResourceCount;

	public override string Description => UIStrings.Instance.SpaceCombatTexts.NavigatorResource;

	public RewardNavigatorResourceUI(RewardNavigatorResource reward)
		: base(reward)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		return new TooltipTemplateSimple(UIStrings.Instance.SpaceCombatTexts.NavigatorResource, UIStrings.Instance.SpaceCombatTexts.NavigatorResourceDescription);
	}
}
