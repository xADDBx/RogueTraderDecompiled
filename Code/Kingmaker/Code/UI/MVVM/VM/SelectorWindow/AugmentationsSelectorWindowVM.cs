using System;
using System.Collections.Generic;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SelectorWindow;

public class AugmentationsSelectorWindowVM : SelectorWindowVM<EquipSelectorSlotVM>
{
	private readonly AugmentationsSlotVM m_EquippedSlot;

	public AugmentationsSelectorWindowVM(Action<EquipSelectorSlotVM> onConfirm, Action onDecline, List<EquipSelectorSlotVM> visibleCollection, AugmentationsSlotVM equippedSlot)
		: base(onConfirm, onDecline, visibleCollection, isAugment: true, (ReactiveProperty<EquipSelectorSlotVM>)null, equippedSlot)
	{
		m_EquippedSlot = equippedSlot;
	}

	public void Unequip()
	{
		InventoryHelper.TryUnequip(m_EquippedSlot);
	}
}
