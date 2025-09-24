using System;
using System.Collections.Generic;
using Kingmaker.Items;
using Kingmaker.UI.Common;

namespace Kingmaker.Code.UI.MVVM.VM.Slots;

public class ItemSlotsGroupVM : SlotsGroupVM<ItemSlotVM>
{
	public ItemSlotsGroupVM(ItemsCollection collection, int slotsInRow, int minSlots, ItemsFilterType filter = ItemsFilterType.NoFilter, ItemsSorterType sorter = ItemsSorterType.NotSorted, bool showUnavailableItems = true, bool showSlotHoldItemsInSlots = false, ItemSlotsGroupType type = ItemSlotsGroupType.Unknown, bool needMaximumLimit = false, int maxSlots = 0)
		: base(collection, slotsInRow, minSlots, (IEnumerable<ItemEntity>)null, filter, sorter, showUnavailableItems, showSlotHoldItemsInSlots, type, (Func<ItemEntity, bool>)null, needMaximumLimit, maxSlots, forceSort: false)
	{
	}

	public ItemSlotsGroupVM(ItemsCollection collection, IEnumerable<ItemEntity> items, int slotsInRow, int minSlots, ItemsFilterType filter = ItemsFilterType.NoFilter, ItemsSorterType sorter = ItemsSorterType.NotSorted, bool showUnavailableItems = true, bool showSlotHoldItemsInSlots = false, ItemSlotsGroupType type = ItemSlotsGroupType.Unknown, bool forceSort = false)
		: base(collection, slotsInRow, minSlots, items, filter, sorter, showUnavailableItems, showSlotHoldItemsInSlots, type, (Func<ItemEntity, bool>)null, needMaximumLimit: false, 0, forceSort)
	{
	}

	public ItemSlotsGroupVM(ItemsCollection collection, IEnumerable<ItemEntity> items, int slotsInRow, int minSlots, bool needMaximumLimit, ItemsFilterType filter = ItemsFilterType.NoFilter, ItemsSorterType sorter = ItemsSorterType.NotSorted, bool showUnavailableItems = true, bool showSlotHoldItemsInSlots = false, ItemSlotsGroupType type = ItemSlotsGroupType.Unknown)
		: base(collection, slotsInRow, minSlots, items, filter, sorter, showUnavailableItems, showSlotHoldItemsInSlots, type, (Func<ItemEntity, bool>)null, needMaximumLimit, 0, forceSort: false)
	{
	}

	protected override ItemSlotVM GetItem(ItemEntity item, int index)
	{
		return new ItemSlotVM(item, index, this);
	}
}
