using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.Colonization;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[AllowedOn(typeof(BlueprintColonyProject))]
[TypeId("1c4c3ee98f054592aa7d83d558d79c74")]
public class RewardChangeNewPassageCost : Reward
{
	public int NewCost;

	public override void ReceiveReward(Colony colony = null)
	{
		Game.Instance.Player.WarpTravelState.CreateNewPassageCost = NewCost;
	}
}
