using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Rewards;

public class RewardModifyWoundDamagePerTurnThresholdHPFractionUI : RewardUI<RewardModifyWoundDamagePerTurnThresholdHPFraction>
{
	public override string Name => string.Empty;

	public override string Description => string.Empty;

	public override Sprite Icon => null;

	public override string NameForAcronym => null;

	public override string CountText => base.Reward.WoundDamagePerTurnThresholdHPFractionModifier.ToString("+#;-#;0");

	public RewardModifyWoundDamagePerTurnThresholdHPFractionUI(RewardModifyWoundDamagePerTurnThresholdHPFraction reward)
		: base(reward)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		return null;
	}
}
