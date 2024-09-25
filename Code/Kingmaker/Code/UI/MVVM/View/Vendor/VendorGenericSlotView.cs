using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorGenericSlotView<TSlot> : ItemSlotView<ItemSlotVM> where TSlot : ItemSlotBaseView
{
	[SerializeField]
	protected TSlot m_ItemSlotView;

	protected void OnClick()
	{
		base.ViewModel.VendorTryMove(split: false);
	}

	protected void OnDoubleClick()
	{
		base.ViewModel.VendorTryBuyAll();
	}
}
