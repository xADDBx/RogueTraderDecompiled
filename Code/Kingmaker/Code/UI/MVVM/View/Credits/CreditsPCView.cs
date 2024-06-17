using Kingmaker.UI.InputSystems;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Credits;

public class CreditsPCView : CreditsBaseView
{
	[Header("PC Input")]
	[SerializeField]
	protected OwlcatMultiButton m_CloseButton;

	private InputLayer m_InputLayer;

	protected override void BindViewImplementation()
	{
		AddDisposable(m_CloseButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.CloseCredits));
		AddDisposable(EscHotkeyManager.Instance.Subscribe(delegate
		{
			base.ViewModel.CloseCredits();
		}));
		base.BindViewImplementation();
		AddDisposable(m_PlayMultiButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.TogglePause));
		AddDisposable(ObservableExtensions.Subscribe(m_ButtonLeft.OnLeftClickAsObservable(), delegate
		{
			OnPrevPage();
		}));
		AddDisposable(ObservableExtensions.Subscribe(m_ButtonRight.OnLeftClickAsObservable(), delegate
		{
			OnNextPage();
		}));
		AddDisposable(m_SearchButton.OnLeftClickAsObservable().Subscribe(base.OnFind));
		CreateInput();
	}

	private void CreateInput()
	{
		m_InputLayer = new InputLayer
		{
			ContextName = "CreditsView"
		};
		m_InputLayer.AddButton(delegate
		{
			OnFind();
		}, 8);
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}
}
