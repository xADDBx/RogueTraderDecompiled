using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Rewards;

public class RewardProfitFactorUI : RewardUI<RewardProfitFactor>
{
	public override string Name => UIStrings.Instance.ProfitFactorTexts.Title;

	public override string Description => UIStrings.Instance.ColonyProjectsRewards.RewardProfitFactor.Text;

	public override Sprite Icon => BlueprintRoot.Instance.UIConfig.UIIcons.ProfitFactor;

	public override string NameForAcronym => null;

	public override int Count => base.Reward.ProfitFactor;

	public override string CountText => base.Reward.ProfitFactor.ToString("+#;-#;0");

	public RewardProfitFactorUI(RewardProfitFactor reward)
		: base(reward)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		return new TooltipTemplateSimple(UIStrings.Instance.ProfitFactorTexts.Title.Text, UIStrings.Instance.ProfitFactorTexts.Description.Text);
	}
}
