using Kingmaker.Code.UI.MVVM.View.Slots;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.UI.MVVM.View.ShipCustomization;

public class ShipComponentSlotPCView : ShipComponentSlotBaseView<ItemSlotPCView>
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ItemSlotPCView.Bind(base.ViewModel);
		m_ItemSlotPCView.IsDraggable = !base.ViewModel.IsLocked.Value;
		AddDisposable(m_ItemSlotPCView.OnSingleLeftClickAsObservable.Subscribe(base.OnClick));
		AddDisposable(m_ItemSlotPCView.OnDoubleClickAsObservable.Subscribe(base.OnDoubleClick));
		AddDisposable(m_ItemSlotPCView.OnPointerEnterAsObservable().Subscribe(delegate
		{
			OnHoverStart();
		}));
		AddDisposable(m_ItemSlotPCView.OnPointerExitAsObservable().Subscribe(delegate
		{
			OnHoverEnd();
		}));
		SetSlotState(isActive: true);
	}
}
