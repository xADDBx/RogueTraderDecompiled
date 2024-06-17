using System;
using Kingmaker.Cargo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Items;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;

public class InventoryDropZoneVM : DropZoneBaseVM
{
	public InventoryDropZoneVM(Action<ItemSlotVM> dropVMAction)
		: base(dropVMAction)
	{
	}

	protected override bool CheckItem(ItemEntity itemEntity)
	{
		if (!CargoHelper.IsItemInCargo(itemEntity))
		{
			if (itemEntity != null)
			{
				return !CargoHelper.IsTrashItem(itemEntity);
			}
			return false;
		}
		return CargoHelper.CanTransferFromCargo(itemEntity);
	}
}
