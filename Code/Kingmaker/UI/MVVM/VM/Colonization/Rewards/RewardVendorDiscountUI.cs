using Kingmaker.Globalmap.Colonization.Rewards;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Colonization.Rewards;

public class RewardVendorDiscountUI : RewardUI<RewardVendorDiscount>
{
	public override string Name => string.Empty;

	public override string Description => string.Empty;

	public override Sprite Icon => null;

	public override string NameForAcronym => null;

	public override string CountText => base.Reward.Discount.ToString();

	public RewardVendorDiscountUI(RewardVendorDiscount reward)
		: base(reward)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		return null;
	}
}
