using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintQuestContract))]
[AllowedOn(typeof(BlueprintAnswer))]
[TypeId("7ed9351cab68434a9d638e5b3a57c466")]
public class RewardCargo : Reward
{
	[SerializeField]
	private BlueprintCargoReference m_Cargo;

	[SerializeField]
	private int m_Count = 1;

	public BlueprintCargo Cargo => m_Cargo?.Get();

	public int Count => m_Count;

	public override void ReceiveReward(Colony colony = null)
	{
		if (Cargo == null)
		{
			PFLog.Default.Error("Empty cargo in RewardCargo");
			return;
		}
		if (colony == null)
		{
			Game.Instance.Player.CargoState.Create(Cargo, m_Count);
			return;
		}
		for (int i = 0; i < m_Count; i++)
		{
			colony.LootToReceive.AddCargo(Cargo.ToReference<BlueprintCargoReference>());
		}
	}
}
