using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("99927183761300749b3c4a75bbaa1a3b")]
public class MovePartyItemsAction : GameAction
{
	[Serializable]
	private class LeaveSettings
	{
		public bool Remote;

		public bool Ex;

		public bool Detached;
	}

	[Flags]
	public enum ItemType
	{
		Weapon = 1,
		Shield = 2,
		Armor = 4,
		Usable = 8,
		Simple = 0x10
	}

	public ItemType PickupTypes = ItemType.Weapon;

	[SerializeReference]
	public ItemsCollectionEvaluator TargetCollection;

	[SerializeField]
	[Tooltip("Do not remove items equipped on some companions")]
	private LeaveSettings m_LeaveEquipmentOf;

	public override string GetCaption()
	{
		return "Pick up party items";
	}

	public override void RunAction()
	{
		if (TargetCollection == null || !TargetCollection.TryGetValue(out var value))
		{
			PFLog.Default.Error(this, $"Не удалось получить ItemsCollection из {TargetCollection}");
			return;
		}
		MoveItemsBetweenCollections(Game.Instance.Player.Inventory, value);
		PartInventory partInventory = Game.Instance.Player?.MainCharacterEntity?.ToBaseUnitEntity().Inventory;
		if (partInventory != null)
		{
			MoveItemsBetweenCollections(partInventory.Collection, value);
		}
	}

	private void MoveItemsBetweenCollections(ItemsCollection sourceCollection, ItemsCollection targetCollection)
	{
		List<ItemEntity> list = ListPool<ItemEntity>.Claim();
		foreach (ItemEntity item7 in sourceCollection.Items)
		{
			if (item7.HoldingSlot != null && item7.HoldingSlot.Owner != null)
			{
				CompanionState? companionState = item7.HoldingSlot.Owner.GetOptional<UnitPartCompanion>()?.State;
				if ((m_LeaveEquipmentOf.Detached && companionState == CompanionState.InPartyDetached) || (m_LeaveEquipmentOf.Remote && companionState == CompanionState.Remote) || (m_LeaveEquipmentOf.Ex && companionState == CompanionState.ExCompanion))
				{
					continue;
				}
			}
			if (!(item7 is ItemEntityWeapon item2))
			{
				if (!(item7 is ItemEntityShield item3))
				{
					if (!(item7 is ItemEntityArmor item4))
					{
						if (!(item7 is ItemEntityUsable item5))
						{
							if (item7 is ItemEntitySimple item6 && PickupTypes.HasFlag(ItemType.Simple))
							{
								list.Add(item6);
							}
						}
						else if (PickupTypes.HasFlag(ItemType.Usable))
						{
							list.Add(item5);
						}
					}
					else if (PickupTypes.HasFlag(ItemType.Armor))
					{
						list.Add(item4);
					}
				}
				else if (PickupTypes.HasFlag(ItemType.Shield))
				{
					list.Add(item3);
				}
			}
			else if (PickupTypes.HasFlag(ItemType.Weapon))
			{
				list.Add(item2);
			}
		}
		list.ForEach(delegate(ItemEntity item)
		{
			sourceCollection.Transfer(item, targetCollection);
		});
	}
}
