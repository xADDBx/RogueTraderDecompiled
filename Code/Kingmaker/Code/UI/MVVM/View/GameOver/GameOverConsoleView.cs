using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;

namespace Kingmaker.Code.UI.MVVM.View.GameOver;

public class GameOverConsoleView : GameOverView
{
	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		CreateInput();
	}

	private void CreateInput()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		m_NavigationBehaviour.SetEntitiesVertical<OwlcatButton>(m_QuickLoadButton, m_LoadButton, m_MainMenuButton, m_IronManDeleteSaveButton, m_IronManContinueGameButton);
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
