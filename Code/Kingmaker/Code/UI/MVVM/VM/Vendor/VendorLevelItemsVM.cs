using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Items;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.Vendor;

public class VendorLevelItemsVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public VendorReputationLevelVM ReputationLevelVM;

	public readonly List<ItemSlotVM> VendorSlots = new List<ItemSlotVM>();

	public VendorLevelItemsVM(int level, bool locked)
	{
		AddDisposable(ReputationLevelVM = new VendorReputationLevelVM(level, locked));
	}

	public void AddItem(ItemEntity item, int index)
	{
		ItemSlotVM itemSlotVM = new ItemSlotVM(item, index);
		AddDisposable(itemSlotVM);
		VendorSlots.Add(itemSlotVM);
	}

	protected override void DisposeImplementation()
	{
	}
}
