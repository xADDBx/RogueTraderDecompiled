using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Code.UI.MVVM.VM.Loot;

public class InsertableLootSlotsGroupVM : SlotsGroupVM<InsertableLootSlotVM>
{
	private readonly Func<ItemEntity, bool> m_CanInsertItem;

	private Func<ItemEntity, bool> CanInsert => (ItemEntity entity) => m_CanInsertItem?.Invoke(entity) ?? true;

	public InsertableLootSlotsGroupVM(ItemsCollection collection, Func<ItemEntity, bool> canInsertItem, int slotsInRow, int minSlots, ItemsFilterType filter = ItemsFilterType.NoFilter, ItemsSorterType sorter = ItemsSorterType.NotSorted, bool showUnavailableItems = true, bool showSlotHoldItemsInSlots = false, ItemSlotsGroupType type = ItemSlotsGroupType.Unknown)
		: base(collection, slotsInRow, minSlots, (IEnumerable<ItemEntity>)null, filter, sorter, showUnavailableItems, showSlotHoldItemsInSlots, type, (Func<ItemEntity, bool>)null, needMaximumLimit: false, 0)
	{
		m_CanInsertItem = canInsertItem;
		base.VisibleCollection.ForEach(delegate(InsertableLootSlotVM item)
		{
			item.UpdateCanInsert();
		});
	}

	protected override InsertableLootSlotVM GetItem(ItemEntity item, int index)
	{
		return new InsertableLootSlotVM(item, index, this, CanInsert);
	}
}
