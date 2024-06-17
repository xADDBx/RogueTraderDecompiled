using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Rewards;

public class RewardModifyWoundsStackForTraumaUI : RewardUI<RewardModifyWoundsStackForTrauma>
{
	public override string Name => string.Empty;

	public override string Description => string.Empty;

	public override Sprite Icon => null;

	public override string NameForAcronym => null;

	public override string CountText => base.Reward.WoundsStackModifier.ToString("+#;-#;0");

	public RewardModifyWoundsStackForTraumaUI(RewardModifyWoundsStackForTrauma reward)
		: base(reward)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		return null;
	}
}
