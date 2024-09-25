using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.Controllers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Enums;
using Kingmaker.Globalmap.Blueprints.Colonization;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintQuestContract))]
[AllowedOn(typeof(BlueprintAnswer))]
[AllowMultipleComponents]
[TypeId("6b753da00905420996556f7cd398267c")]
public class RewardReputation : Reward
{
	[SerializeField]
	public int Reputation;

	[SerializeField]
	public FactionType Faction;

	public override void ReceiveReward(Colony colony = null)
	{
		ReputationHelper.GainFactionReputation(Faction, Reputation);
	}
}
