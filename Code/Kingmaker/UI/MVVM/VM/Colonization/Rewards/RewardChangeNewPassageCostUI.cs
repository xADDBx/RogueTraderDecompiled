using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Rewards;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Colonization.Rewards;

public class RewardChangeNewPassageCostUI : RewardUI<RewardChangeNewPassageCost>
{
	public override string Name => string.Empty;

	public override string Description => UIStrings.Instance.ColonyProjectsRewards.RewardChangeNewPassageCost.Text;

	public override Sprite Icon => null;

	public override string NameForAcronym => null;

	public override string CountText => "-" + base.Reward.NewCost;

	public RewardChangeNewPassageCostUI(RewardChangeNewPassageCost reward)
		: base(reward)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		return new TooltipTemplateSimple(UIStrings.Instance.CommonTexts.Information.Text, UIStrings.Instance.ColonyProjectsRewards.RewardChangeNewPassageCost.Text);
	}
}
