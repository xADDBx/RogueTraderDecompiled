using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[TypeId("9812e3fd225a487aaa342f65a17e4dc9")]
[AllowedOn(typeof(BlueprintQuestContract))]
[AllowedOn(typeof(BlueprintAnswer))]
[AllowMultipleComponents]
public class RewardResourceNotFromColony : Reward
{
	[SerializeField]
	private ResourceData m_ResourceData;

	public BlueprintResource Resource => m_ResourceData?.Resource?.Get();

	public int Count => m_ResourceData?.Count ?? 0;

	public override void ReceiveReward(Colony colony = null)
	{
		if (Resource != null)
		{
			Game.Instance.ColonizationController.AddResourceNotFromColonyToPool(Resource, Count);
		}
	}
}
