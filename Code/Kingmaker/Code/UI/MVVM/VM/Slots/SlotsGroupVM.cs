using System;
using System.Collections.Generic;
using System.Linq;
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

	public ItemsCollection MechanicCollection { get; }

	public AutoDisposingReactiveCollection<TViewModel> VisibleCollection { get; } = new AutoDisposingReactiveCollection<TViewModel>();


	public ReactiveProperty<ItemsFilterType> FilterType { get; } = new ReactiveProperty<ItemsFilterType>();


	public ReactiveProperty<ItemsSorterType> SorterType { get; } = new ReactiveProperty<ItemsSorterType>();


	public ReactiveProperty<string> SearchString { get; } = new ReactiveProperty<string>();


	public ReactiveCommand CollectionChangedCommand { get; } = new ReactiveCommand();


	public List<ItemEntity> Items => m_ItemEntities?.ToList() ?? MechanicCollection?.Items.ToTempList() ?? new List<ItemEntity>();

	public List<ItemEntity> ValidItems => Items.Where(ShouldShowItem).ToList();

	public ItemSlotsGroupType Type { get; }

	protected SlotsGroupVM(ItemsCollection collection, int slotsInRow, int minSlots, IEnumerable<ItemEntity> items = null, ItemsFilterType filter = ItemsFilterType.NoFilter, ItemsSorterType sorter = ItemsSorterType.NotSorted, bool showSlotHoldItemsInSlots = false, ItemSlotsGroupType type = ItemSlotsGroupType.Unknown, Func<ItemEntity, bool> showPredicate = null, bool needMaximumLimit = false, int maxSlots = 0)
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
		FilterType.Value = filter;
		AddDisposable(FilterType.CombineLatest(SorterType, (ItemsFilterType _, ItemsSorterType _) => true).Skip(1).Subscribe(delegate
		{
			UpdateVisibleCollection();
		}));
		AddDisposable(SearchString.Skip(1).Subscribe(delegate
		{
			UpdateVisibleCollection();
		}));
		InternalUpdate();
		AddDisposable(EventBus.Subscribe(this));
	}

	public void SetNewItems(IEnumerable<ItemEntity> items)
	{
		m_ItemEntities = items;
		UpdateVisibleCollection();
	}

	public void UpdateVisibleCollection()
	{
		DelayedInvoker.InvokeAtTheEndOfFrameOnlyOnes(InternalUpdate);
	}

	private void InternalUpdate()
	{
		List<ItemEntity> list = ItemsFilter.ItemSorter(Items.Where(ShouldShowItem).ToList(), SorterType.Value, FilterType.Value);
		if (SorterType.Value != 0)
		{
			for (int j = 0; j < list.Count; j++)
			{
				list[j].SetSlotIndex(j);
			}
		}
		List<EntityIndexPair> list2 = new List<EntityIndexPair>();
		foreach (ItemEntity item in list)
		{
			int inventorySlotIndex = item.InventorySlotIndex;
			if (inventorySlotIndex < list2.Count)
			{
				list2[inventorySlotIndex] = new EntityIndexPair
				{
					Item = item,
					Index = inventorySlotIndex
				};
				continue;
			}
			while (list2.Count < inventorySlotIndex)
			{
				list2.Add(new EntityIndexPair
				{
					Item = null,
					Index = list2.Count
				});
			}
			list2.Add(new EntityIndexPair
			{
				Item = item,
				Index = inventorySlotIndex
			});
		}
		int num = (list.Any() ? (list.Max((ItemEntity i) => i.InventorySlotIndex) + 1) : 0);
		bool removeEmpty = NeedRemoveEmptySlot();
		list2.RemoveAll((EntityIndexPair s) => NeedRemoveSlot(s.Item, removeEmpty));
		int b = ((!m_NeedMaximumLimit) ? (m_SlotsInRow * (Mathf.CeilToInt((float)list2.Count / (float)m_SlotsInRow) + 1)) : m_MaxSlots);
		int num2 = Mathf.Max(m_MinSlots, b);
		while (list2.Count < num2)
		{
			list2.Add(new EntityIndexPair
			{
				Item = null,
				Index = num
			});
			num++;
		}
		SetNewVisibleCollection(list2);
	}

	private bool ShouldShowItem(ItemEntity item)
	{
		if (item == null)
		{
			return false;
		}
		if (item.InventorySlotIndex < 0)
		{
			return false;
		}
		if (Type != ItemSlotsGroupType.Cargo && !item.IsLootable)
		{
			return false;
		}
		if (!m_ShowSlotHoldItems && Type != ItemSlotsGroupType.Cargo && item.HoldingSlot != null)
		{
			return false;
		}
		return true;
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
		if (m_ShowPredicate != null && !m_ShowPredicate(item))
		{
			return true;
		}
		return false;
	}

	private bool NeedRemoveEmptySlot()
	{
		if (SorterType.Value != 0)
		{
			return true;
		}
		if (FilterType.Value != 0)
		{
			return true;
		}
		if (!string.IsNullOrEmpty(SearchString.Value))
		{
			return true;
		}
		if (m_ShowPredicate != null)
		{
			return true;
		}
		return false;
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
			UpdateVisibleCollection();
		}
	}

	protected override void DisposeImplementation()
	{
	}
}
