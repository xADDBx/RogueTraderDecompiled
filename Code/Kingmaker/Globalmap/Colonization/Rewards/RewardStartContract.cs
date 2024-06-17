using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.Designers;
using Kingmaker.Globalmap.Blueprints.Colonization;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintQuestContract))]
[TypeId("8aca928a2b4c46418249e0e714e71ddd")]
public class RewardStartContract : Reward
{
	[SerializeField]
	private BlueprintQuestObjectiveReference m_Objective;

	public BlueprintQuestObjective Objective => m_Objective?.Get();

	public override void ReceiveReward(Colony colony = null)
	{
		GameHelper.Quests.GiveObjective(Objective);
	}
}
