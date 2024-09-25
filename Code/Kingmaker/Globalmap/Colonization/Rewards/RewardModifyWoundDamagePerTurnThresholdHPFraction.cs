using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[TypeId("1b57733e7a114347bab0daeeaa3a5864")]
public class RewardModifyWoundDamagePerTurnThresholdHPFraction : Reward
{
	[Tooltip("Wounds threshold hp for player's party will be greater on value percent")]
	public int WoundDamagePerTurnThresholdHPFractionModifier;

	public override void ReceiveReward(Colony colony = null)
	{
		Game.Instance.Player.TraumasModification.WoundDamagePerTurnThresholdHPFractionModifier += WoundDamagePerTurnThresholdHPFractionModifier;
	}
}
