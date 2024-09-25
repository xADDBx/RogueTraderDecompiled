using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[TypeId("a1beb39017ed442e9da4de4e19440fb1")]
public class RewardModifyOldWoundsDelayRounds : Reward
{
	[Tooltip("old wounds delay rounds for player's party will be greater on value")]
	public int OldWoundsDelayRoundsModifier;

	public override void ReceiveReward(Colony colony = null)
	{
		Game.Instance.Player.TraumasModification.OldWoundDelayRoundsModifier += OldWoundsDelayRoundsModifier;
	}
}
