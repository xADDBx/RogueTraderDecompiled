using Kingmaker.UI.InputSystems;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Loot.PC;

public class ExitLocationWindowPCView : ExitLocationWindowBaseView
{
	[SerializeField]
	protected OwlcatButton m_AcceptButton;

	[SerializeField]
	protected OwlcatButton m_DeclineButton;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(EscHotkeyManager.Instance.Subscribe(base.ViewModel.Decline));
		AddDisposable(m_AcceptButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.Confirm();
		}));
		AddDisposable(m_DeclineButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.Decline();
		}));
	}
}
