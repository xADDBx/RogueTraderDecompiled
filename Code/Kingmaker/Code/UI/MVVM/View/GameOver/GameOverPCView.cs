using Kingmaker.UI.InputSystems;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.GameOver;

public class GameOverPCView : GameOverView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_QuickLoadButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnQuickLoad();
		}));
		AddDisposable(m_LoadButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnButtonLoadGame();
		}));
		AddDisposable(m_MainMenuButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnButtonMainMenu();
		}));
		AddDisposable(base.ViewModel.CanQuickLoad.Subscribe(delegate(bool value)
		{
			m_QuickLoadButton.Interactable = value;
		}));
		AddDisposable(EscHotkeyManager.Instance.Subscribe(delegate
		{
		}));
	}
}
