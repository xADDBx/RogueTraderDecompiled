using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.FlagCountable;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Items.Slots;

[JsonObject(MemberSerialization.OptIn)]
public class ItemSlot : IHashable
{
	public class IgnoreLock : ContextFlag<IgnoreLock>
	{
	}

	[JsonProperty]
	private EntityRef<ItemEntity> m_ItemRef;

	private EntityRef<ItemEntity> m_СutsceneItemRef;

	private readonly CountableFlag m_DeactivateFlag = new CountableFlag();

	public readonly CountableFlag Lock = new CountableFlag();

	[JsonProperty]
	private bool m_Active = true;

	public MechanicEntity Owner { get; private set; }

	[NotNull]
	public ItemEntity Item
	{
		get
		{
			if (MaybeItem == null)
			{
				throw new InvalidOperationException("Has no item in slot");
			}
			return MaybeItem;
		}
	}

	[CanBeNull]
	public ItemEntity MaybeItem => m_СutsceneItemRef.Entity ?? m_ItemRef.Entity;

	public virtual bool HasItem => MaybeItem != null;

	public bool Active
	{
		get
		{
			return m_Active;
		}
		protected set
		{
			if (value != m_Active)
			{
				m_Active = value;
				OnActiveChanged();
			}
		}
	}

	[CanBeNull]
	public PartInventory MaybeOwnerInventory => Owner.GetInventoryOptional();

	[CanBeNull]
	public PartUnitBody MaybeOwnerBody => Owner.GetBodyOptional();

	public bool IsBodyInitializing => MaybeOwnerBody?.IsInitializing ?? false;

	public bool Disabled => m_DeactivateFlag;

	public virtual void UpdateActive()
	{
		Active = !Disabled;
	}

	protected virtual void OnActiveChanged()
	{
		if (Active)
		{
			MaybeItem?.OnDidEquipped(Owner);
		}
		else
		{
			MaybeItem?.OnWillUnequip();
		}
	}

	public ItemSlot(BaseUnitEntity owner)
	{
		Owner = owner;
	}

	public ItemSlot(JsonConstructorMark _)
	{
	}

	public void PrePostLoad(BaseUnitEntity owner)
	{
		Owner = owner;
		ItemEntity maybeItem = MaybeItem;
		if (maybeItem != null)
		{
			maybeItem.HoldingSlot = this;
		}
	}

	public void PostLoad()
	{
		ItemEntity maybeItem = MaybeItem;
		if (maybeItem != null && !maybeItem.IsPostLoadExecuted)
		{
			maybeItem.PostLoad();
		}
		UpdateActive();
	}

	public void PreSave()
	{
		MaybeItem?.PreSave();
	}

	public void Dispose()
	{
		MaybeItem?.Dispose();
	}

	public bool IsPossibleInsertItems()
	{
		if ((bool)ContextData<IgnoreLock>.Current || IsBodyInitializing)
		{
			return true;
		}
		if (Game.Instance.TurnController.TurnBasedModeActive && Owner.IsPlayerFaction)
		{
			return false;
		}
		return !Lock;
	}

	public bool CanInsertItem(ItemEntity item)
	{
		if (item == null || !IsPossibleInsertItems())
		{
			return false;
		}
		if ((bool)ContextData<GameCommandHelper.PreviewItem>.Current)
		{
			return true;
		}
		if (IsItemSupported(item))
		{
			if (!IsBodyInitializing)
			{
				return item.CanBeEquippedBy(Owner);
			}
			return true;
		}
		return false;
	}

	public bool PossibleEquipItem(ItemEntity item)
	{
		if (item != null && IsItemSupported(item))
		{
			if (!IsBodyInitializing)
			{
				return item.CanBeEquippedBy(Owner);
			}
			return true;
		}
		return false;
	}

	public virtual bool IsItemSupported(ItemEntity item)
	{
		return true;
	}

	public bool IsPossibleRemoveItems()
	{
		if (Game.Instance.TurnController.TurnBasedModeActive)
		{
			return !Owner.IsPlayerFaction;
		}
		return true;
	}

	public virtual bool CanRemoveItem()
	{
		if (!ContextData<IgnoreLock>.Current)
		{
			if (IsPossibleRemoveItems() && !Lock)
			{
				return !(MaybeItem?.IsNonRemovable ?? false);
			}
			return false;
		}
		return true;
	}

	public void InsertItem(ItemEntity item, bool force = false)
	{
		if (!force && !CanInsertItem(item))
		{
			LogChannel.Default.Error($"ItemSlot.InsertItem: can't insert item {item} into slot {GetType().Name}");
			return;
		}
		if (item.Collection != null && item.Collection != MaybeOwnerInventory?.Collection)
		{
			PFLog.Default.Error("Item collection the same as owner inventory");
			return;
		}
		if (item.Collection == null && MaybeOwnerInventory != null)
		{
			BlueprintItem blueprint = item.Blueprint;
			item = MaybeOwnerInventory.Add(item);
			if (item == null)
			{
				PFLog.Default.Warning("Item dissipated during insertion. {0}", blueprint);
				return;
			}
		}
		if (item.Wielder == Owner && MaybeOwnerBody == null)
		{
			PFLog.Default.ErrorWithReport("Moving items between slots of MechanicEntity without PartUnitBody isn't supported");
			return;
		}
		item = (item.IsStackable ? item.Split(1) : item);
		if (MaybeItem == item)
		{
			return;
		}
		item.HoldingSlot = this;
		ItemEntity prevItem = MaybeItem;
		if (prevItem != null)
		{
			RemoveItem(raiseEvent: false, autoMerge: true, force);
			prevItem.SetSlotIndex(item.InventorySlotIndex);
		}
		m_ItemRef = item;
		item.UpdateSlotIndex();
		if (item.Wielder == Owner && TryGetOwnerEquipmentSlots(out var equipmentSlots))
		{
			ItemSlot slot = equipmentSlots.First((ItemSlot s) => s.HasItem && s.Item == item);
			ItemEntity pi = slot.MaybeItem;
			slot.m_ItemRef = null;
			EventBus.RaiseEvent((IMechanicEntity)Owner, (Action<IUnitEquipmentHandler>)delegate(IUnitEquipmentHandler h)
			{
				h.HandleEquipmentSlotUpdated(slot, pi);
			}, isCheckRuntime: true);
		}
		else if (Active || (bool)ContextData<GameCommandHelper.PreviewItem>.Current)
		{
			item.OnDidEquipped(Owner);
		}
		MaybeOwnerBody?.OnItemInserted(item);
		OnItemInserted();
		EventBus.RaiseEvent((IMechanicEntity)Owner, (Action<IUnitEquipmentHandler>)delegate(IUnitEquipmentHandler h)
		{
			h.HandleEquipmentSlotUpdated(this, prevItem);
		}, isCheckRuntime: true);
	}

	public void InsertCutsceneItem(ItemEntity item)
	{
		m_СutsceneItemRef = item;
		item.OnDidEquipped(Owner);
		item.HoldingSlot = m_ItemRef.Entity?.HoldingSlot;
	}

	public void RemoveCutsceneItem()
	{
		m_СutsceneItemRef.Entity?.OnWillUnequip();
		m_СutsceneItemRef = null;
		m_СutsceneItemRef.Entity?.OnDidEquipped(Owner);
	}

	public virtual bool RemoveItem(bool autoMerge = true, bool force = false)
	{
		return RemoveItem(raiseEvent: true, autoMerge, force);
	}

	private bool RemoveItem(bool raiseEvent, bool autoMerge, bool force = false)
	{
		ItemEntity item = MaybeItem;
		if (item == null || (!force && !CanRemoveItem() && !ContextData<IgnoreLock>.Current))
		{
			return false;
		}
		if (Active)
		{
			item.OnWillUnequip();
		}
		item.HoldingSlot = null;
		m_ItemRef = null;
		item.UpdateSlotIndex();
		MaybeOwnerBody?.OnItemRemoved(item);
		if (raiseEvent)
		{
			EventBus.RaiseEvent((IMechanicEntity)Owner, (Action<IUnitEquipmentHandler>)delegate(IUnitEquipmentHandler h)
			{
				h.HandleEquipmentSlotUpdated(this, item);
			}, isCheckRuntime: true);
		}
		if (autoMerge)
		{
			item.TryMergeInCollection();
		}
		return true;
	}

	protected virtual void OnItemInserted()
	{
	}

	public void SwitchSlots(ItemSlot slot)
	{
		ItemEntity itemEntity = null;
		ItemEntity itemEntity2 = null;
		if (slot.HasItem)
		{
			itemEntity = slot.Item;
			slot.RemoveItem();
		}
		if (HasItem)
		{
			itemEntity2 = Item;
			RemoveItem();
		}
		if (itemEntity2 != null)
		{
			slot.InsertItem(itemEntity2);
		}
		if (itemEntity != null)
		{
			InsertItem(itemEntity);
		}
	}

	public void RetainDeactivateFlag()
	{
		m_DeactivateFlag.Retain();
		UpdateActive();
	}

	public void ReleaseDeactivateFlag()
	{
		m_DeactivateFlag.Release();
		UpdateActive();
	}

	private bool TryGetOwnerEquipmentSlots(out IEnumerable<ItemSlot> equipmentSlots)
	{
		equipmentSlots = ((!(Owner is StarshipEntity)) ? MaybeOwnerBody?.EquipmentSlots : Owner.GetOptional<PartStarshipHull>()?.HullSlots?.EquipmentSlots);
		return equipmentSlots != null;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		EntityRef<ItemEntity> obj = m_ItemRef;
		Hash128 val = StructHasher<EntityRef<ItemEntity>>.GetHash128(ref obj);
		result.Append(ref val);
		result.Append(ref m_Active);
		return result;
	}
}
