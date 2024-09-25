using System;
using Kingmaker.Cargo;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Items;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;

public class CargoDropZoneVM : DropZoneBaseVM
{
	public CargoDropZoneVM(Action<ItemSlotVM> dropAction)
		: base(dropAction)
	{
	}

	protected override bool CheckItem(ItemEntity itemEntity)
	{
		if (CargoHelper.IsItemInCargo(itemEntity))
		{
			if (CargoHelper.CanTransferToCargo(itemEntity))
			{
				return !Game.Instance.Player.CargoState.LockTransferFromCargo;
			}
			return false;
		}
		return CargoHelper.CanTransferToCargo(itemEntity);
	}
}
