using Kingmaker.UI.InputSystems;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Respec;

public class RespecWindowPCView : RespecWindowCommonView
{
	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	[SerializeField]
	private OwlcatButton m_AcceptButton;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(EscHotkeyManager.Instance.Subscribe(base.CloseWindow));
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			CloseWindow();
		}));
		AddDisposable(m_AcceptButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			OnConfirm();
		}));
		AddDisposable(base.ViewModel.CanRespec.Subscribe(m_AcceptButton.SetInteractable));
	}
}
