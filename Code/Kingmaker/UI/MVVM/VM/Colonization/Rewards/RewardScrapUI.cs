using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Rewards;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Colonization.Rewards;

public class RewardScrapUI : RewardUI<RewardScrap>
{
	public override string Name => string.Empty;

	public override string Description => UIStrings.Instance.ShipCustomization.Scrap;

	public override Sprite Icon => null;

	public override string NameForAcronym => null;

	public override string CountText => base.Reward.Scrap.ToString("+#;-#;0");

	public RewardScrapUI(RewardScrap reward)
		: base(reward)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		return new TooltipTemplateSimple(UIStrings.Instance.ShipCustomization.Scrap, UIStrings.Instance.ShipCustomization.ScrapDescription);
	}
}
