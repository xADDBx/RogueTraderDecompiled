using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.Exploration.Base;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.PC;

public class ExplorationResourcePointPCView : ExplorationResourcePointBaseView
{
	[SerializeField]
	private OwlcatButton m_Button;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_Button.SetHint(base.ViewModel.Name.Value));
		AddDisposable(m_Button.OnLeftClickAsObservable().Subscribe(base.HandleClick));
		AddDisposable(m_Button.OnConfirmClickAsObservable().Subscribe(base.HandleClick));
		AddDisposable(m_Button.OnHoverAsObservable().Subscribe(base.AnimateHover));
		AddDisposable(m_Button.OnFocusAsObservable().Subscribe(base.AnimateHover));
	}

	protected override void SetFocusImpl(bool value)
	{
		m_Button.SetFocus(value);
	}
}
