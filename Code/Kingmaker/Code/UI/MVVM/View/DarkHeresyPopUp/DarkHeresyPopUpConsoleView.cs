using Kingmaker.Blueprints.Root.Strings;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DarkHeresyPopUp;

public class DarkHeresyPopUpConsoleView : DarkHeresyPopUpView
{
	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		BuildNavigation();
		base.BindViewImplementation();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour = null;
		m_InputLayer = null;
	}

	private void BuildNavigation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		m_NavigationBehaviour.AddRow<OwlcatMultiButton>(m_WishlistButton);
		BuildNavigationImpl(m_NavigationBehaviour);
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "DarkHeresyPopUpLayer"
		});
		CreateInputImpl(m_InputLayer);
	}

	private void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		navigationBehaviour.FocusOnFirstValidEntity();
	}

	private void CreateInputImpl(InputLayer inputLayer)
	{
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			Hide();
		}, 9, base.ViewModel.IsVisible);
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.CloseWindow));
		AddDisposable(inputBindStruct);
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			base.ViewModel.OpenStoreToWishlist();
		}, 8, base.ViewModel.IsVisible);
		AddDisposable(m_ConsoleHintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.UIDarkHeresyPopUp.GoToStore));
		AddDisposable(inputBindStruct2);
	}

	public override void Hide()
	{
		base.Hide();
		GamePad.Instance.PopLayer(m_InputLayer);
	}

	public override void Show()
	{
		base.Show();
		GamePad.Instance.PushLayer(m_InputLayer);
		m_NavigationBehaviour.FocusOnLastValidEntity();
	}
}
