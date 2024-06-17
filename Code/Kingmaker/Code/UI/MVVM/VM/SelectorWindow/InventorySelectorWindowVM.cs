using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SelectorWindow;

public class InventorySelectorWindowVM : SelectorWindowVM<EquipSelectorSlotVM>
{
	private readonly EquipSlotVM m_EquippedSlot;

	public InventorySelectorWindowVM(Action<EquipSelectorSlotVM> onConfirm, Action onDecline, List<EquipSelectorSlotVM> visibleCollection, EquipSlotVM equippedSlot)
		: base(onConfirm, onDecline, visibleCollection, (ReactiveProperty<EquipSelectorSlotVM>)null, equippedSlot)
	{
		m_EquippedSlot = equippedSlot;
	}

	public void Unequip()
	{
		if (InventoryHelper.TryUnequip(m_EquippedSlot))
		{
			EventBus.RaiseEvent(delegate(IInventoryHandler h)
			{
				h.Refresh();
			});
		}
	}
}
