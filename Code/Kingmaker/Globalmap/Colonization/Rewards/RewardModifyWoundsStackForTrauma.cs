using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[TypeId("8fc9c4343c1c482bab74aed73a034b01")]
public class RewardModifyWoundsStackForTrauma : Reward
{
	[Tooltip("Wounds stack for player's party will be greater on value")]
	public int WoundsStackModifier;

	public override void ReceiveReward(Colony colony = null)
	{
		Game.Instance.Player.TraumasModification.WoundStacksForTraumaModifier += WoundsStackModifier;
	}
}
