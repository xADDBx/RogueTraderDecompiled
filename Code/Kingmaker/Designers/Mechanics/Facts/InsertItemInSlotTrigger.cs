using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("be41a87b03454036af2a1189972d271d")]
public class InsertItemInSlotTrigger : EntityFactComponentDelegate, IInsertItemHandler, ISubscriber, IEquipItemAutomaticallyHandler, IHashable
{
	[SerializeReference]
	private AbstractUnitEvaluator m_Unit;

	[SerializeField]
	private bool m_TriggerByItemType;

	[SerializeField]
	[HideIf("m_TriggerByItemType")]
	private BlueprintItemReference m_Item;

	[SerializeField]
	[ShowIf("m_TriggerByItemType")]
	private ItemsItemType m_ItemType;

	[SerializeField]
	private ActionList m_Actions;

	private BaseUnitEntity Unit => (m_Unit?.GetValue() as BaseUnitEntity) ?? GameHelper.GetPlayerCharacter();

	private BlueprintItem Item => m_Item.Get();

	public void HandleInsertItem(ItemSlot slot)
	{
		if (slot.MaybeItem == null && slot is HandSlot { IsPrimaryHand: false } handSlot && handSlot.PairSlot.MaybeItem is ItemEntityWeapon { HoldInTwoHands: not false })
		{
			TryRunActions(handSlot.PairSlot.Item);
		}
		else
		{
			TryRunActions(slot.Item);
		}
	}

	public void HandleEquipItemAutomatically(ItemEntity item)
	{
		TryRunActions(item);
	}

	private void TryRunActions(ItemEntity item)
	{
		if (!(item.Owner is BaseUnitEntity baseUnitEntity) || baseUnitEntity == Unit)
		{
			if (m_TriggerByItemType && item.Blueprint.ItemType == m_ItemType)
			{
				m_Actions.Run();
			}
			else if (Item == item.Blueprint)
			{
				m_Actions.Run();
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
