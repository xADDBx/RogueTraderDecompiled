using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[TypeId("45c3f4edb1df41d7b2cc685d71943755")]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintQuestContract))]
[AllowedOn(typeof(BlueprintAnswer))]
public class RewardScrap : Reward
{
	public int Scrap;

	public override void ReceiveReward(Colony colony = null)
	{
		if (Scrap > 0)
		{
			Game.Instance.Player.Scrap.Receive(Scrap);
		}
	}
}
