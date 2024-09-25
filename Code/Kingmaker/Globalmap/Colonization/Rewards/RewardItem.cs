using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[TypeId("1cf6ddd24787436dba7a7e4832eb52ce")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintQuestContract))]
[AllowedOn(typeof(BlueprintAnswer))]
public class RewardItem : Reward
{
	[SerializeField]
	private BlueprintItemReference m_Item;

	[SerializeField]
	private int m_Count;

	public int Count => m_Count;

	public BlueprintItem Item => m_Item?.Get();

	public bool IsMiner
	{
		get
		{
			if (Item != null)
			{
				return Item.ItemType == ItemsItemType.ResourceMiner;
			}
			return false;
		}
	}

	public override void ReceiveReward(Colony colony = null)
	{
		if (Item == null)
		{
			PFLog.Default.Error("Empty item in RewardItem");
			return;
		}
		if (colony != null)
		{
			colony.LootToReceive.AddItem(Item, Count);
			return;
		}
		ItemsCollection inventory = Game.Instance.Player.Inventory;
		if (Item.IsActuallyStackable)
		{
			ItemEntity itemEntity = Item.CreateEntity();
			if (m_Count > 1)
			{
				itemEntity.IncrementCount(m_Count - 1, inventory.ForceStackable);
			}
			inventory.Add(itemEntity);
		}
		else
		{
			for (int i = 0; i < m_Count; i++)
			{
				ItemEntity newItem = Item.CreateEntity();
				inventory.Add(newItem);
			}
		}
	}
}
