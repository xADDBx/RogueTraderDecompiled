using Kingmaker.UI.MVVM.View.Colonization.Base;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.UI.MVVM.View.Colonization.PC;

public class ColonyProjectPCView : ColonyProjectBaseView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.IsSelected.Subscribe(m_MultiButton.SetFocus));
		AddDisposable(m_MultiButton.OnLeftClickAsObservable().Subscribe(base.SelectPage));
	}
}
