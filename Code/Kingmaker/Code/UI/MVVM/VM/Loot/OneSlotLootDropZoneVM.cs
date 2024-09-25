using System;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Items;

namespace Kingmaker.Code.UI.MVVM.VM.Loot;

public class OneSlotLootDropZoneVM : DropZoneBaseVM
{
	private readonly Func<ItemEntity, bool> m_CanInsertItem;

	public OneSlotLootDropZoneVM(Action<ItemSlotVM> dropVMAction, Func<ItemEntity, bool> canInsertItem)
		: base(dropVMAction)
	{
		m_CanInsertItem = canInsertItem;
	}

	protected override bool CheckItem(ItemEntity itemEntity)
	{
		return m_CanInsertItem(itemEntity);
	}
}
