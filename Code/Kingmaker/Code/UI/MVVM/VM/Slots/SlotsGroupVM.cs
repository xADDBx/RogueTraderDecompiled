using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public abstract class SlotsGroupVM<TViewModel> : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ISlotsGroupVM, ITransferItemHandler, ISubscriber, ICollectLootHandler where TViewModel : ItemSlotVM
{
	private struct EntityIndexPair
	{
		public ItemEntity Item;

		public int Index;
	}

	private IEnumerable<ItemEntity> m_ItemEntities;

	private readonly Func<ItemEntity, bool> m_ShowPredicate;

	private readonly int m_MinSlots;

	private readonly int m_MaxSlots;

	private readonly int m_SlotsInRow;

	private readonly bool m_ShowSlotHoldItems;

	private readonly bool m_NeedMaximumLimit;

	private ItemsSorterType m_SorterTypeCache;

	public ItemsCollection MechanicCollection { get; }

	public AutoDisposingReactiveCollection<TViewModel> VisibleCollection { get; } = new AutoDisposingReactiveCollection<TViewModel>();


	public ReactiveProperty<ItemsFilterType> FilterType { get; } = new ReactiveProperty<ItemsFilterType>();


	public ReactiveProperty<ItemsSorterType> SorterType { get; } = new ReactiveProperty<ItemsSorterType>();


	public ReactiveProperty<bool> ShowUnavailable { get; } = new ReactiveProperty<bool>();


	public ReactiveProperty<string> SearchString { get; } = new ReactiveProperty<string>();


	public ReactiveCommand CollectionChangedCommand { get; } = new ReactiveCommand();


	public List<ItemEntity> Items => m_ItemEntities?.ToList() ?? MechanicCollection?.Items.ToTempList() ?? new List<ItemEntity>();

	public List<ItemEntity> ValidItems => Items.Where(ShouldShowItem).ToList();

	public ItemSlotsGroupType Type { get; }

	protected SlotsGroupVM(ItemsCollection collection, int slotsInRow, int minSlots, IEnumerable<ItemEntity> items = null, ItemsFilterType filter = ItemsFilterType.NoFilter, ItemsSorterType sorter = ItemsSorterType.NotSorted, bool showUnavailableItems = true, bool showSlotHoldItemsInSlots = false, ItemSlotsGroupType type = ItemSlotsGroupType.Unknown, Func<ItemEntity, bool> showPredicate = null, bool needMaximumLimit = false, int maxSlots = 0, bool forceSort = false)
	{
		MechanicCollection = collection;
		m_ItemEntities = items;
		Type = type;
		m_ShowPredicate = showPredicate;
		m_MinSlots = minSlots;
		m_MaxSlots = maxSlots;
		m_SlotsInRow = slotsInRow;
		m_ShowSlotHoldItems = showSlotHoldItemsInSlots;
		m_NeedMaximumLimit = needMaximumLimit;
		SorterType.Value = sorter;
		m_SorterTypeCache = SorterType.Value;
		FilterType.Value = filter;
		ShowUnavailable.Value = showUnavailableItems;
		AddDisposable(FilterType.Skip(1).Subscribe(delegate
		{
			UpdateVisibleCollection();
		}));
		AddDisposable(SorterType.Skip(1).Subscribe(delegate
		{
			UpdateVisibleCollection();
		}));
		AddDisposable(ShowUnavailable.Skip(1).Subscribe(delegate
		{
			UpdateVisibleCollection();
		}));
		AddDisposable(SearchString.Skip(1).Subscribe(delegate
		{
			UpdateVisibleCollection();
		}));
		InternalUpdate(forceSort);
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	public void SetNewItems(IEnumerable<ItemEntity> items)
	{
		m_ItemEntities = items;
		UpdateVisibleCollection();
	}

	public void UpdateVisibleCollection()
	{
		MainThreadDispatcher.StartCoroutine(InvokeAtEndOfFrameCoroutine());
	}

	private IEnumerator InvokeAtEndOfFrameCoroutine()
	{
		yield return null;
		InternalUpdate();
	}

	private void InternalUpdate(bool force = false)
	{
		List<ItemEntity> list = Items.Where(ShouldShowItem).ToList();
		List<ItemEntity> list2 = list;
		if (m_SorterTypeCache != SorterType.Value || force)
		{
			list2 = ItemsFilter.ItemSorter(list, SorterType.Value, FilterType.Value);
		}
		list2.RemoveAll((ItemEntity item) => item != null && !ItemsFilter.ShouldShowItem(item, SorterType.Value));
		if (!ShowUnavailable.Value)
		{
			list2.RemoveAll((ItemEntity i) => !UIUtilityItem.IsEquipPossible(i) && !UIUtilityItem.IsQuestItem(i?.Blueprint));
		}
		if ((SorterType.Value != 0 && m_SorterTypeCache != SorterType.Value) || force || RootUIContext.Instance.IsLootShow)
		{
			for (int j = 0; j < list2.Count; j++)
			{
				list2[j].SetSlotIndex(j);
			}
		}
		List<EntityIndexPair> list3 = new List<EntityIndexPair>();
		foreach (ItemEntity item in list2)
		{
			int inventorySlotIndex = item.InventorySlotIndex;
			if (inventorySlotIndex < list3.Count)
			{
				list3[inventorySlotIndex] = new EntityIndexPair
				{
					Item = item,
					Index = inventorySlotIndex
				};
				continue;
			}
			while (list3.Count < inventorySlotIndex)
			{
				list3.Add(new EntityIndexPair
				{
					Item = null,
					Index = list3.Count
				});
			}
			list3.Add(new EntityIndexPair
			{
				Item = item,
				Index = inventorySlotIndex
			});
		}
		int num = (list2.Any() ? (list2.Max((ItemEntity i) => i.InventorySlotIndex) + 1) : 0);
		bool removeEmpty = NeedRemoveEmptySlot();
		list3.RemoveAll((EntityIndexPair s) => NeedRemoveSlot(s.Item, removeEmpty));
		int b = ((!m_NeedMaximumLimit) ? (m_SlotsInRow * (Mathf.CeilToInt((float)list3.Count / (float)m_SlotsInRow) + 1)) : m_MaxSlots);
		int num2 = Mathf.Max(m_MinSlots, b);
		while (list3.Count < num2)
		{
			list3.Add(new EntityIndexPair
			{
				Item = null,
				Index = num
			});
			num++;
		}
		SetNewVisibleCollection(list3);
		m_SorterTypeCache = SorterType.Value;
	}

	public void SortItems()
	{
		InternalUpdate(force: true);
	}

	private bool ShouldShowItem(ItemEntity item)
	{
		if (item != null)
		{
			MechanicEntity owner = item.Owner;
			if (owner == null || !owner.IsDisposed)
			{
				if (item.InventorySlotIndex < 0)
				{
					return false;
				}
				if (Type != ItemSlotsGroupType.Cargo && !item.IsLootable)
				{
					return false;
				}
				if (!m_ShowSlotHoldItems && Type != ItemSlotsGroupType.Cargo)
				{
					return item.HoldingSlot == null;
				}
				return true;
			}
		}
		return false;
	}

	private bool NeedRemoveSlot(ItemEntity item, bool removeEmpty)
	{
		if (item == null)
		{
			return removeEmpty;
		}
		if (!ItemsFilter.ShouldShowItem(item, FilterType.Value))
		{
			return true;
		}
		if (!ItemsFilter.IsMatchSearchRequest(item, SearchString.Value))
		{
			return true;
		}
		if (m_ShowPredicate != null)
		{
			return !m_ShowPredicate(item);
		}
		return false;
	}

	private bool NeedRemoveEmptySlot()
	{
		if (m_SorterTypeCache == SorterType.Value)
		{
			return false;
		}
		if (SorterType.Value != 0)
		{
			return true;
		}
		if (!string.IsNullOrEmpty(SearchString.Value))
		{
			return true;
		}
		return m_ShowPredicate != null;
	}

	private void SetNewVisibleCollection(List<EntityIndexPair> newCollection)
	{
		if (newCollection.Empty())
		{
			VisibleCollection.Clear();
			return;
		}
		for (int i = 0; i < newCollection.Count; i++)
		{
			EntityIndexPair entityIndexPair = newCollection[i];
			if (VisibleCollection.Count <= i)
			{
				VisibleCollection.Add(GetVirtualSlot(entityIndexPair.Item, entityIndexPair.Index));
				continue;
			}
			VisibleCollection[i].Index = entityIndexPair.Index;
			VisibleCollection[i].SetItem(entityIndexPair.Item);
		}
		while (VisibleCollection.Count > newCollection.Count)
		{
			int index = VisibleCollection.Count - 1;
			VisibleCollection[index].Dispose();
			RemoveDisposable(VisibleCollection[index]);
			VisibleCollection.RemoveAt(index);
		}
		CollectionChangedCommand.Execute();
	}

	private TViewModel GetVirtualSlot(ItemEntity item, int index)
	{
		TViewModel item2 = GetItem(item, index);
		AddDisposable(item2);
		return item2;
	}

	protected abstract TViewModel GetItem(ItemEntity item, int index);

	void ITransferItemHandler.HandleTransferItem(ItemsCollection from, ItemsCollection to)
	{
		if (from == MechanicCollection || to == MechanicCollection)
		{
			UpdateVisibleCollection();
		}
	}

	void ICollectLootHandler.HandleCollectAll(ItemsCollection from, ItemsCollection to)
	{
		if (from == MechanicCollection || to == MechanicCollection)
		{
			DelayedInvoker.InvokeInFrames(UpdateVisibleCollection, 5);
		}
	}
}
