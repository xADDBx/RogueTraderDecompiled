using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Cargo;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.View.Mechadendrites;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.GameCommands;

public static class GameCommandHelper
{
	public class PreviewItem : ContextFlag<PreviewItem>
	{
	}

	private static void SetItem(ItemEntity item, int index)
	{
		if (item != null)
		{
			int oldItemIndex = item.InventorySlotIndex;
			item.SetSlotIndex(index);
			EventBus.RaiseEvent(delegate(IInventoryChangedHandler h)
			{
				h.HandleSetItem(item, oldItemIndex);
			});
		}
	}

	private static void Split(ItemSlotRef from, ItemSlotRef to, int count, bool isLoot)
	{
		ItemEntity newItem = from.Item.Split(count);
		if (newItem != null)
		{
			EventBus.RaiseEvent(delegate(ISplitItemHandler h)
			{
				h.HandleSplitItem();
			});
			if (to.ItemsCollection != from.ItemsCollection && isLoot)
			{
				TransferCount(to.ItemsCollection, newItem, newItem.Count);
			}
			SetItem(newItem, to.SlotIndex);
			EventBus.RaiseEvent(delegate(IInventoryChangedHandler h)
			{
				h.HandleUpdateItem(from.Item, from.Item.Collection, from.SlotIndex);
			});
			EventBus.RaiseEvent(delegate(IInventoryChangedHandler h)
			{
				h.HandleUpdateItem(newItem, to.ItemsCollection, to.SlotIndex);
			});
		}
	}

	private static bool TryRemove(this ItemSlot slot, bool autoMerge = true)
	{
		if (!slot.HasItem)
		{
			return false;
		}
		if (slot.CanRemoveItem())
		{
			return slot.RemoveItem(autoMerge);
		}
		return false;
	}

	public static void DropItem(ItemEntity item, bool split, int splitCount)
	{
		ItemEntity item2 = (split ? item.Split(splitCount) : item);
		if (item.Collection.DropItem(item2))
		{
			EventBus.RaiseEvent(delegate(IDropItemHandler h)
			{
				h.HandleDropItem(item, split);
			});
		}
	}

	public static void UnequipItem(MechanicEntity owner, ItemSlotRef from, ItemSlotRef to)
	{
		if (!TryGetEquipSlot(owner, from, out var itemSlot) || !itemSlot.HasItem)
		{
			return;
		}
		ItemEntity item = itemSlot.Item;
		if (!itemSlot.TryRemove())
		{
			return;
		}
		EventBus.RaiseEvent(delegate(IUnequipItemHandler h)
		{
			h.HandleUnequipItem();
		});
		if (to != null)
		{
			ItemsCollection collection = item.Collection;
			ItemEntity itemEntity = ((collection != null) ? collection.Items.FirstOrDefault((ItemEntity x) => x.InventorySlotIndex == to.SlotIndex) : null);
			if (itemEntity == null)
			{
				SetItem(item, to.SlotIndex);
			}
			else
			{
				int inventorySlotIndex = item.InventorySlotIndex;
				int inventorySlotIndex2 = itemEntity.InventorySlotIndex;
				SetItem(itemEntity, inventorySlotIndex);
				SetItem(item, inventorySlotIndex2);
			}
			EventBus.RaiseEvent(delegate(IMoveItemHandler h)
			{
				h.HandleMoveItem(isEquip: true);
			});
		}
	}

	public static void TrySplitSlot(ItemSlotRef fromData, ItemSlotRef toData, bool isLoot, int count)
	{
		SplitSlot(fromData, toData, isLoot, count);
	}

	private static void SplitSlot(ItemSlotRef from, ItemSlotRef to, bool isLoot, int count)
	{
		if (to == null)
		{
			from.Item.Split(count);
			EventBus.RaiseEvent(delegate(ISplitItemHandler h)
			{
				h.HandleAfterSplitItem(from.Item);
			});
			return;
		}
		if (!isLoot)
		{
			EventBus.RaiseEvent(delegate(ISplitItemHandler h)
			{
				h.HandleBeforeSplitItem(from.Item, from.ItemsCollection, to.ItemsCollection);
			});
		}
		Split(from, to, count, isLoot);
		if (!isLoot)
		{
			EventBus.RaiseEvent(delegate(ISplitItemHandler h)
			{
				h.HandleAfterSplitItem(from.Item);
			});
		}
		EventBus.RaiseEvent(delegate(IMoveItemHandler h)
		{
			h.HandleMoveItem(from.IsEquipment || to.IsEquipment);
		});
	}

	public static void TrySwapSlots(ItemSlotRef fromData, ItemSlotRef toData, MechanicEntity entity, bool isLoot)
	{
		if (fromData.ItemsCollection != toData.ItemsCollection && isLoot)
		{
			ItemEntity item = fromData.Item;
			int slotIndex = toData.SlotIndex;
			ItemsCollection itemsCollection = toData.ItemsCollection;
			TransferCount(toData.ItemsCollection, fromData.Item, item.Count);
			SwapSlots(item.ToSlotRef(), itemsCollection.ToSlotRef(slotIndex), entity, isLoot: true);
		}
		else
		{
			SwapSlots(fromData, toData, entity, isLoot);
		}
	}

	private static void SwapSlots(ItemSlotRef fromData, ItemSlotRef toData, MechanicEntity entity, bool isLoot)
	{
		ItemEntity item = fromData.Item;
		ItemEntity item2 = toData.Item;
		if (isLoot || (!fromData.IsEquipment && !toData.IsEquipment))
		{
			int slotIndex = fromData.SlotIndex;
			int slotIndex2 = toData.SlotIndex;
			SetItem(item, slotIndex2);
			SetItem(item2, slotIndex);
			EventBus.RaiseEvent(delegate(IMoveItemHandler h)
			{
				h.HandleMoveItem(fromData.IsEquipment || toData.IsEquipment);
			});
			return;
		}
		bool autoMerge = !(entity is StarshipEntity) || !fromData.IsEquipment || !toData.IsEquipment;
		TryGetEquipSlot(entity, fromData, out var itemSlot);
		TryGetEquipSlot(entity, toData, out var itemSlot2);
		if (fromData.IsEquipment)
		{
			if ((itemSlot2 != null && !itemSlot2.CanInsertItem(item)) || (itemSlot != null && !itemSlot.TryRemove(autoMerge)))
			{
				EventBus.RaiseEvent(delegate(IMoveItemHandler h)
				{
					h.HandleMoveItem(fromData.IsEquipment || toData.IsEquipment);
				});
				return;
			}
		}
		else
		{
			SetItem(item2, fromData.SlotIndex);
		}
		if (toData.IsEquipment)
		{
			if (!TryInsertItem(itemSlot2, item))
			{
				EventBus.RaiseEvent(delegate(IMoveItemHandler h)
				{
					h.HandleMoveItem(fromData.IsEquipment || toData.IsEquipment);
				});
				return;
			}
			if (itemSlot != null && !itemSlot.HasItem && item2 != null)
			{
				TryInsertItem(itemSlot, item2);
			}
		}
		else
		{
			SetItem(item, toData.SlotIndex);
		}
		EventBus.RaiseEvent(delegate(IMoveItemHandler h)
		{
			h.HandleMoveItem(fromData.IsEquipment || toData.IsEquipment);
		});
	}

	public static bool TryInsertItem(ItemSlot slot, ItemEntity item)
	{
		bool num = TryInsertItemTo(item, slot);
		if (num)
		{
			EventBus.RaiseEvent(delegate(IInsertItemHandler h)
			{
				h.HandleInsertItem(slot);
			});
		}
		return num;
	}

	public static void EquipItemAutomatically(ItemEntity item, BaseUnitEntity unit)
	{
		if (TryEquipItemAutomatically(item, unit) && ContextData<PreviewItem>.Current == null)
		{
			EventBus.RaiseEvent(delegate(IEquipItemAutomaticallyHandler h)
			{
				h.HandleEquipItemAutomatically(item);
			});
		}
	}

	private static bool TryEquipItemAutomatically(ItemEntity item, BaseUnitEntity unit)
	{
		if (unit is StarshipEntity ship)
		{
			return TryEquipStarshipItem(ship, item);
		}
		if (item is ItemEntityWeapon weapon)
		{
			return TryEquipSlotWeapon(weapon, unit);
		}
		if (item is ItemEntityShield)
		{
			return TryEquipShield(item, unit);
		}
		if (item?.Blueprint is BlueprintItemEquipmentRing)
		{
			return TryEquipRing(item, unit);
		}
		if (item is ItemEntityUsable)
		{
			return TryEquipQuickSlot(item, unit);
		}
		return TryEquipToSuitableSlot(item, unit);
	}

	private static bool TryEquipSlotWeapon(ItemEntityWeapon weapon, BaseUnitEntity unit)
	{
		if (weapon == null || unit == null)
		{
			return false;
		}
		HandsEquipmentSet currentHandsEquipmentSet = unit.Body.CurrentHandsEquipmentSet;
		HandSlot targetSlot = ((!unit.HasMechadendrites()) ? (TryGetSlotForWeapon(weapon, unit) ?? currentHandsEquipmentSet.PrimaryHand) : (weapon.Blueprint.IsMelee ? currentHandsEquipmentSet.PrimaryHand : currentHandsEquipmentSet.SecondaryHand));
		return TryInsertItemTo(weapon, targetSlot);
	}

	private static HandSlot TryGetSlotForWeapon(ItemEntityWeapon weapon, BaseUnitEntity unit)
	{
		HandSlot result = null;
		bool holdInTwoHands = weapon.HoldInTwoHands;
		foreach (HandsEquipmentSet handsEquipmentSet in unit.Body.HandsEquipmentSets)
		{
			HandSlot primaryHand = handsEquipmentSet.PrimaryHand;
			HandSlot secondaryHand = handsEquipmentSet.SecondaryHand;
			ItemEntityWeapon maybeWeapon = primaryHand.MaybeWeapon;
			if (maybeWeapon != null && maybeWeapon.HoldInTwoHands)
			{
				continue;
			}
			bool flag = !primaryHand.HasItem && !secondaryHand.HasItem;
			if (!holdInTwoHands || flag)
			{
				if (!primaryHand.HasItem && primaryHand.CanInsertItem(weapon))
				{
					result = primaryHand;
					break;
				}
				if (!secondaryHand.HasItem && secondaryHand.CanInsertItem(weapon))
				{
					result = secondaryHand;
					break;
				}
			}
		}
		return result;
	}

	private static bool TryEquipShield(ItemEntity shield, BaseUnitEntity unit)
	{
		if (shield == null || unit == null)
		{
			return false;
		}
		HandSlot targetSlot = unit.Body.CurrentHandsEquipmentSet?.SecondaryHand;
		return TryInsertItemTo(shield, targetSlot);
	}

	private static bool TryEquipRing(ItemEntity ring, BaseUnitEntity unit)
	{
		if (ring == null || unit == null)
		{
			return false;
		}
		EquipmentSlot<BlueprintItemEquipmentRing> ring2 = unit.Body.Ring1;
		EquipmentSlot<BlueprintItemEquipmentRing> ring3 = unit.Body.Ring2;
		EquipmentSlot<BlueprintItemEquipmentRing> targetSlot = ((!ring2.HasItem) ? ring2 : (ring3.HasItem ? ring2 : ring3));
		return TryInsertItemTo(ring, targetSlot);
	}

	private static bool TryEquipQuickSlot(ItemEntity quick, BaseUnitEntity unit)
	{
		if (quick == null || unit == null)
		{
			return false;
		}
		IEnumerable<UsableSlot> quickSlots = unit.Body.QuickSlots;
		IEnumerable<UsableSlot> source = (quickSlots as UsableSlot[]) ?? quickSlots.ToArray();
		UsableSlot targetSlot = source.FirstOrDefault((UsableSlot sl) => !sl.HasItem) ?? source.First();
		return TryInsertItemTo(quick, targetSlot);
	}

	private static bool TryEquipToSuitableSlot(ItemEntity item, BaseUnitEntity unit)
	{
		if (item == null || unit == null)
		{
			return false;
		}
		ItemSlot itemSlot = unit.Body.AllSlots.FirstOrDefault((ItemSlot sl) => sl.CanInsertItem(item));
		if (itemSlot != null)
		{
			return TryInsertItemTo(item, itemSlot);
		}
		return false;
	}

	private static bool TryEquipStarshipItem(StarshipEntity ship, ItemEntity item)
	{
		ItemSlot shipEquipSlot = GetShipEquipSlot(ship, item);
		if (shipEquipSlot != null)
		{
			return TryInsertItemTo(item, shipEquipSlot);
		}
		return false;
	}

	private static ItemSlot GetShipEquipSlot(StarshipEntity ship, ItemEntity item)
	{
		if (item == null || ship == null)
		{
			return null;
		}
		BlueprintItem blueprint = item.Blueprint;
		if (!(blueprint is BlueprintItemPlasmaDrives))
		{
			if (!(blueprint is BlueprintItemVoidShieldGenerator))
			{
				if (!(blueprint is BlueprintItemWarpDrives))
				{
					if (!(blueprint is BlueprintItemGellerFieldDevice))
					{
						if (!(blueprint is BlueprintItemLifeSustainer))
						{
							if (!(blueprint is BlueprintItemBridge))
							{
								if (!(blueprint is BlueprintItemAugerArray))
								{
									if (!(blueprint is BlueprintItemArmorPlating))
									{
										if (!(blueprint is BlueprintItemArsenal))
										{
											if (blueprint is BlueprintStarshipWeapon)
											{
												IEnumerable<Warhammer.SpaceCombat.StarshipLogic.Weapon.WeaponSlot> source = ship.Hull.WeaponSlots.Where((Warhammer.SpaceCombat.StarshipLogic.Weapon.WeaponSlot slot) => slot.CanInsertItem(item));
												return source.FirstOrDefault((Warhammer.SpaceCombat.StarshipLogic.Weapon.WeaponSlot weaponSlot) => !weaponSlot.HasItem) ?? source.FirstOrDefault();
											}
											return null;
										}
										IEnumerable<ArsenalSlot> source2 = ship.Hull.HullSlots.Arsenals.Where((ArsenalSlot slot) => slot.CanInsertItem(item));
										return source2.FirstOrDefault((ArsenalSlot slot) => !slot.HasItem) ?? source2.FirstOrDefault();
									}
									return ship.Hull.HullSlots.ArmorPlating;
								}
								return ship.Hull.HullSlots.AugerArray;
							}
							return ship.Hull.HullSlots.Bridge;
						}
						return ship.Hull.HullSlots.LifeSustainer;
					}
					return ship.Hull.HullSlots.GellerFieldDevice;
				}
				return ship.Hull.HullSlots.WarpDrives;
			}
			return ship.Hull.HullSlots.VoidShieldGenerator;
		}
		return ship.Hull.HullSlots.PlasmaDrives;
	}

	private static void RaiseHandleInsertFail(MechanicEntity owner)
	{
		if (ContextData<PreviewItem>.Current == null)
		{
			EventBus.RaiseEvent(delegate(IInsertItemFailHandler h)
			{
				h.HandleInsertFail(owner);
			});
		}
	}

	private static bool TryInsertItemTo(ItemEntity item, ItemSlot targetSlot)
	{
		if (item == null)
		{
			RaiseHandleInsertFail(targetSlot?.Owner);
			return false;
		}
		if (targetSlot == null)
		{
			RaiseHandleInsertFail(null);
			return false;
		}
		if (!targetSlot.CanInsertItem(item) || (targetSlot.HasItem && !targetSlot.CanRemoveItem()) || (item.HoldingSlot != null && item.HoldingSlot.Owner != targetSlot.Owner))
		{
			RaiseHandleInsertFail(targetSlot.Owner);
			return false;
		}
		targetSlot.InsertItem(item);
		return true;
	}

	public static void TransferCount(ItemsCollection to, ItemEntity item, int count)
	{
		ItemsCollection from = item.Collection;
		if (from == to)
		{
			return;
		}
		int oldIndex = item.InventorySlotIndex;
		bool num = count >= item.Count;
		item.Collection.Transfer(item, count, to);
		if (num)
		{
			EventBus.RaiseEvent(delegate(IInventoryChangedHandler h)
			{
				h.HandleRemoveItem(item, from, oldIndex);
			});
		}
		EventBus.RaiseEvent(delegate(IInventoryChangedHandler h)
		{
			h.HandleUpdateItem(item, item.Collection, item.InventorySlotIndex);
		});
		EventBus.RaiseEvent(delegate(ITransferItemHandler h)
		{
			h.HandleTransferItem(from, to);
		});
	}

	public static void RemoveFromBuy(ItemEntity item, int count)
	{
		Game.Instance.Vendor.RemoveFromBuy(item, count);
	}

	public static void AddForBuy(ItemEntity item, int count)
	{
		Game.Instance.Vendor.AddForBuy(item, count);
	}

	public static void TryCollect(List<EntityRef<ItemEntity>> items)
	{
		ItemsCollection from = null;
		bool flag = false;
		foreach (EntityRef<ItemEntity> item in items)
		{
			if (item.Entity == null)
			{
				continue;
			}
			from = item.Entity.Collection;
			if (CargoHelper.IsTrashItem(item))
			{
				if (!CargoHelper.CanTransferToCargo(item) || CargoHelper.IsItemInCargo(item))
				{
					continue;
				}
				Game.Instance.Player.CargoState.AddToCargo(item);
			}
			else
			{
				if (from == Game.Instance.Player.Inventory)
				{
					continue;
				}
				from.Transfer(item, Game.Instance.Player.Inventory);
			}
			flag = true;
		}
		if (flag)
		{
			EventBus.RaiseEvent(delegate(ICollectLootHandler h)
			{
				h.HandleCollectAll(from, Game.Instance.Player.Inventory);
			});
		}
	}

	public static void MergeSlot(ItemSlotRef from, ItemSlotRef to)
	{
		if (to.ItemsCollection != from.ItemsCollection)
		{
			TransferCount(to.ItemsCollection, from.Item, from.Item.Count);
		}
		ItemEntity item = to.Item;
		if (item != null && item.TryMerge(from.Item))
		{
			EventBus.RaiseEvent(delegate(IInventoryChangedHandler h)
			{
				h.HandleUpdateItem(null, from.ItemsCollection, from.SlotIndex);
			});
		}
		EventBus.RaiseEvent(delegate(IMoveItemHandler h)
		{
			h.HandleMoveItem(from.IsEquipment || to.IsEquipment);
		});
	}

	public static void SetCurrentQuest(Quest newQuest)
	{
		Quest currentQuest = Game.Instance.Player.UISettings.CurrentQuest;
		if (currentQuest == newQuest)
		{
			PFLog.Default.Log("CurrentQuest already is " + ((newQuest == null) ? "null" : newQuest?.Blueprint?.name));
			return;
		}
		if (currentQuest != null)
		{
			foreach (QuestObjective objective in currentQuest.Objectives)
			{
				if (!objective.IsVisible || objective.State == QuestObjectiveState.None || objective.Blueprint.IsAddendum)
				{
					continue;
				}
				objective.NeedToAttention = false;
				foreach (QuestObjective item in (from b in objective?.Blueprint?.Addendums?.Where((BlueprintQuestObjective b) => b != null)
					select objective?.Quest?.TryGetObjective(b) into a
					where a != null
					where a.IsVisible
					orderby a?.Order descending
					select a))
				{
					item.NeedToAttention = false;
				}
			}
		}
		Game.Instance.Player.UISettings.CurrentQuest = newQuest;
		EventBus.RaiseEvent(delegate(ISetCurrentQuestHandler h)
		{
			h.HandleSetCurrentQuest(newQuest);
		});
	}

	public static bool TryGetEquipSlot([NotNull] MechanicEntity entity, [NotNull] ItemSlotRef slotRef, out ItemSlot itemSlot)
	{
		itemSlot = null;
		if (!slotRef.IsEquipment)
		{
			return false;
		}
		if (entity is StarshipEntity starshipEntity)
		{
			itemSlot = starshipEntity.Hull.HullSlots.GetEquipShipSlot(slotRef.ShipComponentSlotType, slotRef.SetIndex);
		}
		else
		{
			itemSlot = entity.GetBodyOptional()?.GetEquipSlot(slotRef.EquipSlotType, slotRef.SetIndex);
		}
		return itemSlot != null;
	}
}
