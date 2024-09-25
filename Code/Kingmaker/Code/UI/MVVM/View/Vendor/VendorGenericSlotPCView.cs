using Kingmaker.Code.UI.MVVM.View.Slots;
using Owlcat.Runtime.UniRx;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorGenericSlotPCView : VendorGenericSlotView<ItemSlotPCView>
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ItemSlotView.IsDraggable = false;
		m_ItemSlotView.Bind(base.ViewModel);
		AddDisposable(m_ItemSlotView.OnLeftClickAsObservable.Subscribe(base.OnClick));
		AddDisposable(m_ItemSlotView.OnDoubleClickAsObservable.Subscribe(base.OnDoubleClick));
	}
}
