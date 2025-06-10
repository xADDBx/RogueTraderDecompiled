using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Cargo;
using Kingmaker.Code.UI.MVVM.VM.CounterWindow;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Interaction;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using UniRx;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.StarshipLogic;
using Warhammer.SpaceCombat.StarshipLogic.Equipment;

namespace Kingmaker.UI.Common;

public static class InventoryHelper
{
	private static ItemSlot s_ItemSlot;

	private static ItemEntity s_Item;

	private static Action s_ItemCallback;

	public static ItemsCollectionRef ToCollectionRef(this ItemsCollection itemsCollection, LootFromPointOfInterestHolderRef lootHolderRef = null)
	{
		return new ItemsCollectionRef(itemsCollection, lootHolderRef);
	}

	public static ItemSlotRef ToSlotRef(this ItemsCollection itemsCollection, int index)
	{
		return new ItemSlotRef(EquipSlotType.PrimaryHand, -1, index, itemsCollection.FirstOrDefault((ItemEntity x) => x.InventorySlotIndex == index), itemsCollection, ShipComponentSlotType.PlasmaDrives);
	}

	public static ItemSlotRef ToSlotRef(this ItemEntity itemEntity)
	{
		return new ItemSlotRef(EquipSlotType.PrimaryHand, -1, itemEntity.InventorySlotIndex, itemEntity, itemEntity.Collection, ShipComponentSlotType.PlasmaDrives);
	}

	public static ItemSlotRef ToSlotRef(this ItemSlotVM slotVM)
	{
		if (slotVM is EquipSlotVM equipSlotVM)
		{
			return new ItemSlotRef(equipSlotVM.SlotType, equipSlotVM.SetIndex, -1, equipSlotVM.ItemEntity, slotVM.ItemEntity?.Collection ?? equipSlotVM.ItemSlot?.MaybeOwnerInventory?.Collection, ShipComponentSlotType.PlasmaDrives);
		}
		if (slotVM is ShipComponentSlotVM shipComponentSlotVM)
		{
			return shipComponentSlotVM.ToSlotRef();
		}
		ItemsCollection itemsCollection = slotVM.ItemEntity?.Collection ?? slotVM.Group?.MechanicCollection;
		LootFromPointOfInterestHolderRef lootHolderRef = GetLootHolderRef(itemsCollection);
		return new ItemSlotRef(EquipSlotType.PrimaryHand, -1, slotVM.Index, slotVM.ItemEntity, itemsCollection, ShipComponentSlotType.PlasmaDrives, lootHolderRef);
	}

	private static LootFromPointOfInterestHolderRef GetLootHolderRef(this ItemSlotVM slotVM)
	{
		return GetLootHolderRef(slotVM.ItemEntity?.Collection ?? slotVM.Group?.MechanicCollection);
	}

	private static LootFromPointOfInterestHolderRef GetLootHolderRef(ItemsCollection slotCollection)
	{
		if (!(slotCollection?.Owner is StarSystemObjectEntity starSystemObjectEntity))
		{
			return null;
		}
		LootFromPointOfInterestHolder lootFromPointOfInterestHolder = starSystemObjectEntity.LootHolder.FirstOrDefault((LootFromPointOfInterestHolder x) => x.Items == slotCollection);
		if (lootFromPointOfInterestHolder != null)
		{
			return new LootFromPointOfInterestHolderRef(starSystemObjectEntity, lootFromPointOfInterestHolder);
		}
		return null;
	}

	public static ItemSlotRef ToSlotRef(this ShipComponentSlotVM shipComponentSlotVM)
	{
		return new ItemSlotRef(EquipSlotType.PrimaryHand, shipComponentSlotVM.SetIndex, -1, shipComponentSlotVM.ItemEntity, shipComponentSlotVM.ItemEntity?.Collection ?? shipComponentSlotVM.ItemSlot?.MaybeOwnerInventory?.Collection, shipComponentSlotVM.SlotType);
	}

	public static bool TryUnequip(EquipSlotVM slot)
	{
		ItemSlot itemSlot = slot.ItemSlot;
		if (Game.Instance.Player.IsInCombat)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.EquipInCombatIsImpossible, addToLog: false, WarningNotificationFormat.Short);
			});
			return false;
		}
		if (itemSlot == null || !itemSlot.HasItem || (itemSlot.HasItem && !itemSlot.CanRemoveItem()))
		{
			return false;
		}
		if (!(itemSlot.Owner is BaseUnitEntity baseUnitEntity))
		{
			return false;
		}
		if (!baseUnitEntity.CanBeControlled() && !baseUnitEntity.IsStarship())
		{
			return false;
		}
		Game.Instance.GameCommandQueue.UnequipItem(baseUnitEntity, slot.ToSlotRef(), null);
		UISounds.Instance.PlayItemSound(SlotAction.Take, slot.ItemEntity, equipSound: true);
		return true;
	}

	public static bool TryUnequip(ShipComponentSlotVM slot)
	{
		ItemSlot itemSlot = slot.ItemSlot;
		if (Game.Instance.Player.IsInCombat)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.EquipInCombatIsImpossible, addToLog: false, WarningNotificationFormat.Short);
			});
			return false;
		}
		if (itemSlot == null || !itemSlot.HasItem || (itemSlot.HasItem && !itemSlot.CanRemoveItem()))
		{
			return false;
		}
		if (!(itemSlot.Owner is BaseUnitEntity baseUnitEntity))
		{
			return false;
		}
		if (!baseUnitEntity.CanBeControlled() && !baseUnitEntity.IsStarship())
		{
			return false;
		}
		if (slot.SlotType == ShipComponentSlotType.PlasmaDrives)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.ShipCustomization.PlaceEngineSlot.Text, addToLog: false);
			});
			return false;
		}
		Game.Instance.GameCommandQueue.UnequipItem(baseUnitEntity, slot.ToSlotRef(), null);
		UISounds.Instance.PlayItemSound(SlotAction.Take, slot.ItemEntity, equipSound: true, baseUnitEntity.IsStarship());
		return true;
	}

	public static bool TryMoveToCargo(ItemSlotVM slot)
	{
		if (Game.Instance.Player.IsInCombat)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.EquipInCombatIsImpossible, addToLog: false, WarningNotificationFormat.Short);
			});
			return false;
		}
		if (slot == null || !slot.HasItem)
		{
			return false;
		}
		ItemEntity value = slot.Item.Value;
		if (value == null)
		{
			return false;
		}
		Game.Instance.GameCommandQueue.TransferItemsToCargo(new List<EntityRef<ItemEntity>> { value });
		UISounds.Instance.PlayItemSound(SlotAction.Take, slot.ItemEntity, equipSound: true);
		return true;
	}

	public static bool TryMoveToInventory(ItemSlotVM slot)
	{
		ItemEntity value = slot.Item.Value;
		if (value == null)
		{
			return false;
		}
		Game.Instance.GameCommandQueue.TransferItemsToInventory(new List<EntityRef<ItemEntity>> { value });
		return true;
	}

	private static bool CanEquipItemInCombat(ItemEntity item)
	{
		bool num = item is ItemEntityWeapon;
		bool flag = item is ItemEntityShield;
		bool flag2 = item is ItemEntityUsable;
		return num || flag || flag2;
	}

	public static bool CanChangeEquipment(BaseUnitEntity unit)
	{
		return !Game.Instance.TurnController.TurnBasedModeActive;
	}

	public static bool CanEquipItem(ItemEntity item, BaseUnitEntity unit)
	{
		if (!CanChangeEquipment(unit))
		{
			return false;
		}
		if (Game.Instance.Player.IsInCombat)
		{
			return CanEquipItemInCombat(item);
		}
		return true;
	}

	public static void TryEquip(ItemSlotVM slot, BaseUnitEntity unit)
	{
		if (unit == null || slot == null || !slot.HasItem || (unit is UnitEntity && !unit.CanBeControlled()))
		{
			return;
		}
		ItemEntity itemEntity = slot.ItemEntity;
		if (unit.IsPet && itemEntity.Blueprint.ItemType != ItemsItemType.PetProtocol)
		{
			return;
		}
		if (!CanEquipItem(itemEntity, unit))
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.EquipInCombatIsImpossible, addToLog: false, WarningNotificationFormat.Short);
			});
			return;
		}
		Game.Instance.GameCommandQueue.EquipItem(itemEntity, unit, null);
		bool isNotable = itemEntity.Blueprint.IsNotable;
		if (UIUtilityItem.GetEquipPosibility(itemEntity)[0] || isNotable || itemEntity is ItemEntitySimple)
		{
			UISounds.Instance.PlayItemSound(SlotAction.Put, itemEntity, equipSound: true);
		}
		else
		{
			UISounds.Instance.Sounds.Combat.CombatGridCantPerformActionClick.Play();
		}
	}

	public static void TryDrop(ItemSlotVM slot)
	{
		if (slot != null && slot.HasItem)
		{
			ItemEntity value = slot.Item.Value;
			if (value != null)
			{
				TryDrop(value);
			}
		}
	}

	public static void TryDrop(ItemEntity item)
	{
		if (Game.Instance.Player.IsInCombat)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.EquipInCombatIsImpossible, addToLog: false, WarningNotificationFormat.Short);
			});
		}
		else if (UIUtility.IsGlobalMap())
		{
			s_Item = item;
			s_ItemCallback = delegate
			{
				EventBus.RaiseEvent(delegate(IDropItemHandler h)
				{
					h.HandleDropItem(item, isSplit: false);
				});
			};
			UIUtility.ShowMessageBox(UIStrings.Instance.CommonTexts.DropItemFromGlobalMap, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
			{
				if (button == DialogMessageBoxBase.BoxButton.Yes)
				{
					_ = s_ItemSlot;
				}
			});
		}
		else
		{
			DropItemMechanic(item);
		}
	}

	private static void DropItemMechanic(ItemEntity item, bool showCounterWindow = false)
	{
		if (showCounterWindow && item.IsStackable && item.Count > 1)
		{
			EventBus.RaiseEvent(delegate(ICounterWindowUIHandler h)
			{
				h.HandleOpen(CounterWindowType.Drop, item, delegate(int count)
				{
					Game.Instance.GameCommandQueue.DropItem(item, split: true, count);
				});
			});
		}
		else
		{
			Game.Instance.GameCommandQueue.DropItem(item, split: false, 0);
		}
	}

	public static void TrySplitSlot(ItemSlotVM slot, bool isLoot)
	{
		if (!slot.HasItem || !slot.ItemEntity.IsStackable || slot.ItemEntity.Count <= 1)
		{
			return;
		}
		EventBus.RaiseEvent(delegate(ICounterWindowUIHandler h)
		{
			h.HandleOpen(CounterWindowType.Split, slot.Item.Value, delegate(int count)
			{
				Game.Instance.GameCommandQueue.SplitSlot(slot.ToSlotRef(), null, isLoot, count);
			});
		});
	}

	public static bool CanTransferFromCargo(this CargoEntity fromCargo, ItemEntity item)
	{
		if (!fromCargo.Blueprint.Integral && fromCargo.Blueprint.CanRemoveItems)
		{
			return CargoHelper.CanTransferFromCargo(item);
		}
		return false;
	}

	public static void TryMoveSlotInInventory(ItemSlotVM from, ItemSlotVM to)
	{
		CargoEntity cargoEntity = from.Group?.MechanicCollection.Owner as CargoEntity;
		CargoEntity cargoEntity2 = to.Group?.MechanicCollection.Owner as CargoEntity;
		ReactiveProperty<ItemEntity> item = from.Item;
		bool fromShip = item != null && item.Value.Origin == ItemsItemOrigin.ShipComponents;
		if ((from.Group != to.Group && cargoEntity != null) || cargoEntity2 != null)
		{
			if (cargoEntity != null && !cargoEntity.CanTransferFromCargo(from.ItemEntity))
			{
				if (cargoEntity.Blueprint.Integral)
				{
					PFLog.UI.Log("Cannot transfer items from this cargo cause Integral true");
				}
				else if (!CargoHelper.CanTransferFromCargo(from.ItemEntity))
				{
					EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
					{
						h.HandleWarning(UIStrings.Instance.CargoTexts.TrashItemCargo.Text, addToLog: false, WarningNotificationFormat.Short);
					});
					PFLog.UI.Log($"Cannot transfer items from this cargo cause {from.ItemEntity} is trash item");
				}
				else
				{
					PFLog.UI.Log("Cannot transfer items from this cargo cause CanRemoveItems false");
				}
				return;
			}
			if (cargoEntity2 != null && !cargoEntity2.CanAdd(from.ItemEntity, out var _))
			{
				if (cargoEntity2.Blueprint.Integral)
				{
					PFLog.UI.Log("Cannot add to cargo cause Integral true");
				}
				else if (!cargoEntity2.CorrectOrigin(from.ItemEntity.Blueprint.Origin))
				{
					PFLog.UI.Log("Cannot add to cargo cause item origin not correct");
				}
				else
				{
					PFLog.UI.Log("Cannot add to cargo cause is is full");
				}
				UISounds.Instance.Sounds.Combat.CombatGridCantPerformActionClick.Play();
				return;
			}
		}
		if (Game.Instance.Player.IsInCombat)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.EquipInCombatIsImpossible, addToLog: false, WarningNotificationFormat.Short);
			});
			return;
		}
		if (to is EquipSlotVM equipSlotVM)
		{
			if (!equipSlotVM.ItemSlot.Owner.CanBeControlled())
			{
				return;
			}
			if (from.Group != to.Group)
			{
				if (!equipSlotVM.ItemSlot.CanInsertItem(from.Item.Value))
				{
					EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
					{
						h.HandleWarning(UIStrings.Instance.ShipCustomization.CantInsertInThisWeaponSlot.Text, addToLog: false, WarningNotificationFormat.Short);
					});
				}
				Game.Instance.GameCommandQueue.EquipItem(from.Item.Value, equipSlotVM.ItemSlot.Owner, equipSlotVM.ToSlotRef());
				return;
			}
		}
		if (from.Group != to.Group && to is ShipComponentSlotVM shipComponentSlotVM)
		{
			Game.Instance.GameCommandQueue.EquipItem(from.Item.Value, shipComponentSlotVM.ItemSlot.Owner, shipComponentSlotVM.ToSlotRef());
			return;
		}
		if (from.Group != to.Group && (bool)(to.Item?.Value?.Blueprint as BlueprintStarshipItem))
		{
			Game.Instance.GameCommandQueue.EquipItem(to.Item?.Value, from.ItemEntity?.Owner, from.ToSlotRef());
			return;
		}
		if (from is EquipSlotVM equipSlotVM2)
		{
			if (!equipSlotVM2.ItemSlot.Owner.CanBeControlled())
			{
				return;
			}
			if (from.Group != to.Group)
			{
				MechanicEntity owner = equipSlotVM2.ItemSlot.Owner;
				Game.Instance.GameCommandQueue.UnequipItem(owner, equipSlotVM2.ToSlotRef(), to.ToSlotRef());
				return;
			}
		}
		if (from.Group != to.Group && from is ShipComponentSlotVM shipComponentSlotVM2)
		{
			Game.Instance.GameCommandQueue.UnequipItem(shipComponentSlotVM2.ItemSlot.Owner, shipComponentSlotVM2.ToSlotRef(), to.ToSlotRef());
			return;
		}
		bool isLootOrCargo = cargoEntity != null || cargoEntity2 != null;
		ProcessDragEnd(from, to, isLootOrCargo, fromShip);
	}

	private static void ProcessDragEnd(ItemSlotVM from, ItemSlotVM to, bool isLootOrCargo, bool fromShip)
	{
		ItemSlotRef fromRef = from.ToSlotRef();
		ItemSlotRef toRef = to.ToSlotRef();
		switch (GetEndDragAction(from, to))
		{
		case UIUtility.EndDragAction.Put:
		case UIUtility.EndDragAction.Swap:
			if (from.Group == to.Group && from.Group != null && !fromShip)
			{
				from.Group.SorterType.Value = ItemsSorterType.NotSorted;
			}
			Game.Instance.GameCommandQueue.SwapSlots(from.ItemEntity.Owner, fromRef, toRef, isLootOrCargo);
			break;
		case UIUtility.EndDragAction.Split:
			EventBus.RaiseEvent(delegate(ICounterWindowUIHandler h)
			{
				h.HandleOpen(CounterWindowType.Split, from.Item.Value, delegate(int count)
				{
					Game.Instance.GameCommandQueue.SplitSlot(fromRef, toRef, isLootOrCargo, count);
				});
			});
			break;
		case UIUtility.EndDragAction.HalfSplit:
			Game.Instance.GameCommandQueue.SplitSlot(fromRef, toRef, isLootOrCargo, from.Item.Value.Count / 2);
			break;
		case UIUtility.EndDragAction.Merge:
			Game.Instance.GameCommandQueue.MergeSlot(fromRef, toRef);
			break;
		case UIUtility.EndDragAction.Abort:
			break;
		}
	}

	private static UIUtility.EndDragAction GetEndDragAction(ItemSlotVM from, ItemSlotVM to)
	{
		if (from == to)
		{
			return UIUtility.EndDragAction.Abort;
		}
		if (to.HasItem && to.Item.Value.CanBeMerged(from.Item.Value))
		{
			return UIUtility.EndDragAction.Merge;
		}
		if (from.Item.Value.IsStackable && from.Item.Value.Count > 1 && !to.HasItem)
		{
			if (KeyboardAccess.IsShiftHold())
			{
				return UIUtility.EndDragAction.Split;
			}
			if (KeyboardAccess.IsCtrlHold())
			{
				return UIUtility.EndDragAction.HalfSplit;
			}
		}
		if (!to.HasItem)
		{
			return UIUtility.EndDragAction.Put;
		}
		return UIUtility.EndDragAction.Swap;
	}

	public static bool TryCollectLootSlot(ItemSlotVM slot)
	{
		if (slot == null || !slot.HasItem)
		{
			return false;
		}
		Game.Instance.GameCommandQueue.CollectLoot(new List<EntityRef<ItemEntity>>
		{
			new EntityRef<ItemEntity>(slot.ItemEntity)
		});
		return true;
	}

	public static void InsertToInteractionSlot(InsertableLootSlotVM fromSlot, InteractionSlotPartVM toSlot)
	{
		if (toSlot != null)
		{
			ItemsCollection obj = fromSlot.ItemEntity?.Collection ?? fromSlot.Group.MechanicCollection;
			ItemsCollection mechanicCollection = toSlot.Group.MechanicCollection;
			if (obj != null && mechanicCollection != null && fromSlot.CanInsert.Value)
			{
				TryCollectLootSlot(toSlot.ItemSlot.Value);
				Game.Instance.GameCommandQueue.TransferItem(fromSlot.ItemEntity, mechanicCollection.ToCollectionRef(), 1);
			}
		}
	}

	public static void TryMoveSlot(ItemSlotVM from, ItemSlotVM to, InteractionSlotPartVM interactionSlot)
	{
		if (from.Group != to.Group && from is InsertableLootSlotVM fromSlot)
		{
			InsertToInteractionSlot(fromSlot, interactionSlot);
		}
		else if (from.Group == to.Group || from.HasItem)
		{
			ProcessDragEnd(from, to, isLootOrCargo: true, fromShip: false);
		}
	}

	public static void TryTransferInventorySlot(ItemSlotVM slot, LootObjectVM contextLoot)
	{
		SlotsGroupVM<ItemSlotVM> slotsGroupVM = contextLoot?.SlotsGroup;
		if (slotsGroupVM == null)
		{
			return;
		}
		ItemEntity itemEntity = slot.ItemEntity;
		if (itemEntity != null)
		{
			ItemsCollection mechanicCollection = slotsGroupVM.MechanicCollection;
			if (itemEntity.Collection != mechanicCollection)
			{
				LootFromPointOfInterestHolderRef lootHolderRef = GetLootHolderRef(mechanicCollection);
				ItemsCollectionRef to = mechanicCollection.ToCollectionRef(lootHolderRef);
				Game.Instance.GameCommandQueue.TransferItem(itemEntity, to, itemEntity.Count);
			}
		}
	}

	public static ItemSlot GetEquipSlot(this PartUnitBody partUnitBody, EquipSlotType type, int handSetIndex)
	{
		switch (type)
		{
		case EquipSlotType.Armor:
			return partUnitBody.Armor;
		case EquipSlotType.PrimaryHand:
			return partUnitBody.HandsEquipmentSets[handSetIndex].PrimaryHand;
		case EquipSlotType.SecondaryHand:
			return partUnitBody.HandsEquipmentSets[handSetIndex].SecondaryHand;
		case EquipSlotType.Belt:
			return partUnitBody.Belt;
		case EquipSlotType.Head:
			return partUnitBody.Head;
		case EquipSlotType.Feet:
			return partUnitBody.Feet;
		case EquipSlotType.Gloves:
			return partUnitBody.Gloves;
		case EquipSlotType.Neck:
			return partUnitBody.Neck;
		case EquipSlotType.Ring1:
			return partUnitBody.Ring1;
		case EquipSlotType.Ring2:
			return partUnitBody.Ring2;
		case EquipSlotType.Wrist:
			return partUnitBody.Wrist;
		case EquipSlotType.Shoulders:
			return partUnitBody.Shoulders;
		case EquipSlotType.PetProtocol:
			return partUnitBody.PetProtocol;
		case EquipSlotType.Glasses:
			return partUnitBody.Glasses;
		case EquipSlotType.Shirt:
			return partUnitBody.Shirt;
		case EquipSlotType.QuickSlot1:
		case EquipSlotType.QuickSlot2:
		case EquipSlotType.QuickSlot3:
		case EquipSlotType.QuickSlot4:
		case EquipSlotType.QuickSlot5:
			return partUnitBody.QuickSlots[handSetIndex];
		default:
			throw new ArgumentOutOfRangeException("type", type, null);
		}
	}

	public static ItemSlot GetEquipShipSlot(this HullSlots partUnitBody, ShipComponentSlotType type, int handSetIndex)
	{
		return type switch
		{
			ShipComponentSlotType.ArmorPlating => partUnitBody.ArmorPlating, 
			ShipComponentSlotType.AugerArray => partUnitBody.AugerArray, 
			ShipComponentSlotType.PlasmaDrives => partUnitBody.PlasmaDrives, 
			ShipComponentSlotType.VoidShieldGenerator => partUnitBody.VoidShieldGenerator, 
			ShipComponentSlotType.Arsenal => partUnitBody.Arsenals[handSetIndex], 
			ShipComponentSlotType.Dorsal => partUnitBody.WeaponSlots[handSetIndex], 
			ShipComponentSlotType.Prow1 => partUnitBody.WeaponSlots[handSetIndex], 
			ShipComponentSlotType.Prow2 => partUnitBody.WeaponSlots[handSetIndex], 
			ShipComponentSlotType.Port => partUnitBody.WeaponSlots[handSetIndex], 
			ShipComponentSlotType.Starboard => partUnitBody.WeaponSlots[handSetIndex], 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}

	public static bool NeedOrder(this ItemsSorterType sorterType)
	{
		return sorterType != ItemsSorterType.NotSorted;
	}

	public static bool NeedOrder(this ISlotsGroupVM group)
	{
		return group?.SorterType.NeedOrder() ?? false;
	}

	public static bool NeedOrder(this ReactiveProperty<ItemsSorterType> sorterProperty)
	{
		return sorterProperty.Value.NeedOrder();
	}
}
