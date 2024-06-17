using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.Colonization;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[AllowedOn(typeof(BlueprintColonyProject))]
[TypeId("86422803a1504f879ecf6c13d5b63abb")]
public class RewardAllRoutesNotDeadly : Reward
{
	public override void ReceiveReward(Colony colony = null)
	{
		Game.Instance.SectorMapController.TryChangeDifficultyNotDeadly();
	}
}
