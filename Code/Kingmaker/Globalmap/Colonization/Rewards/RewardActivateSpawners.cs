using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.View.Spawners;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintQuestContract))]
[TypeId("9b7199fa7cbf4ad1ab8b763180d49ea6")]
public class RewardActivateSpawners : Reward
{
	[SerializeField]
	[AllowedEntityType(typeof(UnitSpawnerBase))]
	private List<EntityReference> m_Spawners;

	[SerializeField]
	private RewardActivateSpawnersType m_Type;

	public RewardActivateSpawnersType Type => m_Type;

	public override void ReceiveReward(Colony colony = null)
	{
		if (m_Spawners != null)
		{
			Game.Instance.Player.ActivatedSpawners.AddRange(m_Spawners);
		}
	}
}
