using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.ShipCustomization;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SelectorWindow;

public class ShipItemSelectorWindowVM : SelectorWindowVM<ShipComponentItemSlotVM>
{
	public readonly ShipComponentSlotVM EquippedSlot;

	public ShipItemSelectorWindowVM(Action<ShipComponentItemSlotVM> onConfirm, Action onDecline, List<ShipComponentItemSlotVM> visibleCollection, ShipComponentSlotVM shipSlot)
		: base(onConfirm, onDecline, visibleCollection, (ReactiveProperty<ShipComponentItemSlotVM>)null, (EquipSlotVM)null)
	{
		EquippedSlot = shipSlot;
	}

	public void Unequip()
	{
		if (InventoryHelper.TryUnequip(EquippedSlot))
		{
			EventBus.RaiseEvent(delegate(IInventoryHandler h)
			{
				h.Refresh();
			});
		}
	}
}
