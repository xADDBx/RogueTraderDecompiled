using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.Colonization.Base;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;

namespace Kingmaker.UI.MVVM.View.Colonization.PC;

public class ColonyEventPCView : ColonyEventBaseView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_Button.OnLeftClickAsObservable().Subscribe(HandleButtonClick));
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip));
	}

	private void HandleButtonClick()
	{
		base.ViewModel.HandleColonyEvent();
	}
}
