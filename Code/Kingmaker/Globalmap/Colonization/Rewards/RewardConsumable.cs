using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintQuestContract))]
[TypeId("84b99720cae34d2f93405bff58fbbfb7")]
public class RewardConsumable : Reward
{
	[SerializeField]
	private BlueprintItemReference m_Item;

	[SerializeField]
	private int m_MaxCount;

	[SerializeField]
	private int m_SegmentsToRefill;

	private BlueprintColonyReference m_Colony;

	public int MaxCount => m_MaxCount;

	public BlueprintItem Item => m_Item?.Get();

	public int SegmentsToRefill => m_SegmentsToRefill;

	public BlueprintColony Colony => m_Colony?.Get();

	public override void ReceiveReward(Colony colony = null)
	{
		if (Colony == null && colony == null)
		{
			return;
		}
		if (Colony != null)
		{
			colony = Game.Instance.Player.ColoniesState.Colonies.FirstOrDefault((ColoniesState.ColonyData colonyData) => colonyData.Colony.Blueprint == Colony)?.Colony;
		}
		colony?.Consumables.Add(new Consumable
		{
			Item = Item,
			LastRefill = Game.Instance.TimeController.GameTime,
			MaxCount = MaxCount,
			SegmentsToRefill = SegmentsToRefill
		});
	}
}
