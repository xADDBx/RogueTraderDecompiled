using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.GameOver;

public class GameOverConsoleView : GameOverView
{
	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_QuickLoadButton.OnConfirmClickAsObservable().Subscribe(base.ViewModel.OnQuickLoad));
		AddDisposable(m_LoadButton.OnConfirmClickAsObservable().Subscribe(base.ViewModel.OnButtonLoadGame));
		AddDisposable(m_MainMenuButton.OnConfirmClickAsObservable().Subscribe(base.ViewModel.OnButtonMainMenu));
		AddDisposable(base.ViewModel.CanQuickLoad.Subscribe(delegate(bool value)
		{
			m_QuickLoadButton.Interactable = value;
		}));
		CreateInput();
	}

	private void CreateInput()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		m_NavigationBehaviour.SetEntitiesVertical<OwlcatButton>(m_QuickLoadButton, m_LoadButton, m_MainMenuButton);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "GameOverConsoleView"
		});
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		m_NavigationBehaviour.FocusOnFirstValidEntity();
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}
}
