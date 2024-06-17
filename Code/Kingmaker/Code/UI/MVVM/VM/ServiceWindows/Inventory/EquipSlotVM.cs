using System;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.View.Mechadendrites;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;

public class EquipSlotVM : ItemSlotVM, IEquipSlotPossibleTarget, ISubscriber<ItemEntity>, ISubscriber, IInsertItemHandler, IEquipSlotHoverHandler, INetRoleSetHandler
{
	public readonly EquipSlotType SlotType;

	public readonly EquipSlotSubtype SlotSubtype;

	public readonly ReactiveProperty<bool> CanBeFakeItem = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> NetLock = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsNotRemovable = new ReactiveProperty<bool>(initialValue: false);

	private readonly ReactiveProperty<ItemEntity> m_FakeItem = new ReactiveProperty<ItemEntity>();

	private readonly ItemSlot m_ItemSlot;

	public ItemSlot ItemSlot => m_ItemSlot;

	public int SetIndex { get; }

	public EquipSlotVM(EquipSlotType slotType, ItemSlot itemSlot, int index = -1, EquipSlotVM primaryHand = null, int setIndex = -1)
		: base(itemSlot.MaybeItem, index)
	{
		SlotType = slotType;
		m_ItemSlot = itemSlot;
		SetIndex = setIndex;
		if (itemSlot.Owner.HasMechadendrites() && slotType == EquipSlotType.SecondaryHand)
		{
			SlotSubtype = EquipSlotSubtype.Ranged;
		}
		if (primaryHand != null)
		{
			m_FakeItem = primaryHand.Item;
			AddDisposable(m_FakeItem.Subscribe(delegate
			{
				Icon.Value = GetIcon();
			}));
			AddDisposable(m_FakeItem.CombineLatest(Item, (ItemEntity fake, ItemEntity item) => new { fake, item }).Subscribe(value =>
			{
				CanBeFakeItem.Value = value.fake != null && value.item == null;
			}));
		}
		NetLock.Value = itemSlot.Owner is BaseUnitEntity entry && !entry.CanBeControlled();
		AddDisposable(Item.Subscribe(delegate(ItemEntity item)
		{
			IsNotRemovable.Value = item?.IsNonRemovable ?? false;
		}));
	}

	protected override Sprite GetIcon()
	{
		ItemEntity value = m_FakeItem.Value;
		if (base.HasItem || !(value is ItemEntityWeapon { HoldInTwoHands: not false } itemEntityWeapon))
		{
			return base.GetIcon();
		}
		if (!itemEntityWeapon.HoldInTwoHands)
		{
			return value.Icon;
		}
		return itemEntityWeapon.Icon;
	}

	public void UpdateSlotData()
	{
		Item.Value = m_ItemSlot.MaybeItem;
	}

	protected override int GetItemsCount()
	{
		return 1;
	}

	protected override void DisposeImplementation()
	{
		m_FakeItem.Value = null;
		base.DisposeImplementation();
	}

	public bool InsertItem(ItemEntity item)
	{
		if (m_ItemSlot == null || !m_ItemSlot.CanInsertItem(item) || (m_ItemSlot.HasItem && !m_ItemSlot.CanRemoveItem()))
		{
			EventBus.RaiseEvent((IItemEntity)item, (Action<IInsertItemFailHandler>)delegate(IInsertItemFailHandler h)
			{
				h.HandleInsertFail(m_ItemSlot?.Owner);
			}, isCheckRuntime: true);
			return false;
		}
		m_ItemSlot.InsertItem(item);
		Item.Value = item;
		return true;
	}

	public bool TryUnequip()
	{
		if (m_ItemSlot == null || !m_ItemSlot.HasItem || (m_ItemSlot.HasItem && !m_ItemSlot.CanRemoveItem()))
		{
			return false;
		}
		return m_ItemSlot.RemoveItem();
	}

	public void HandleHighlightStart(ItemEntity item)
	{
		IsPossibleHighlighted = IsPossibleTarget(item);
		UpdatePossibleTarget();
	}

	public new void HandleHighlightStop()
	{
		IsPossibleHighlighted = false;
		UpdatePossibleTarget();
	}

	public void HandleHoverStart(ItemEntity item)
	{
		IsPossibleHovered = IsPossibleTarget(item);
		UpdatePossibleTarget();
	}

	public new void HandleHoverStop()
	{
		IsPossibleHovered = false;
		UpdatePossibleTarget();
	}

	private bool IsPossibleTarget(ItemEntity item)
	{
		if (m_ItemSlot.CanInsertItem(item))
		{
			if (m_ItemSlot.HasItem)
			{
				return m_ItemSlot.CanRemoveItem();
			}
			return true;
		}
		return false;
	}

	private void UpdatePossibleTarget()
	{
		PossibleTarget.Value = IsPossibleHighlighted || IsPossibleHovered;
	}

	void IInsertItemHandler.HandleInsertItem(ItemSlot slot)
	{
		if (m_ItemSlot == slot)
		{
			Item.Value = m_ItemSlot.Item;
		}
	}

	public void HandleRoleSet(string entityId)
	{
		if (!(ItemSlot.Owner.UniqueId != entityId))
		{
			NetLock.Value = ItemSlot.Owner is BaseUnitEntity entry && !entry.CanBeControlled();
		}
	}
}
