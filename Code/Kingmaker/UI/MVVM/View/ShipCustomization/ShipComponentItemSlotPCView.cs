using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.UI.MVVM.View.ShipCustomization;

public class ShipComponentItemSlotPCView : ShipComponentItemSlotBaseView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_Button.OnLeftClickAsObservable().Subscribe(base.OnClick));
		AddDisposable(base.ViewModel.IsSelected.Subscribe(delegate(bool value)
		{
			m_Button.SetActiveLayer(value ? 1 : 0);
		}));
	}
}
