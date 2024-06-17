using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Portrait;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Console.Phases.Appearance.Components.Portrait;

public class CharGenCustomPortraitCreatorConsoleView : CharGenCustomPortraitCreatorView
{
	[SerializeField]
	private ConsoleHint m_DeclineHint;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	protected override void BindViewImplementation()
	{
		CreateNavigation();
		base.BindViewImplementation();
	}

	private void CreateNavigation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "CharGenCustomPortraitCreatorConsoleView"
		});
		m_NavigationBehaviour.AddEntityVertical(m_OpenFolderButton);
		m_NavigationBehaviour.AddEntityVertical(m_RefreshPortraitButton);
		AddDisposable(m_OpenFolderButton.OnConfirmClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnOpenFolderClick();
		}));
		AddDisposable(m_RefreshPortraitButton.OnConfirmClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnRefreshPortraitClick();
		}));
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(delegate
		{
			base.ViewModel.OnClose();
		}, 9);
		AddDisposable(m_DeclineHint.Bind(inputBindStruct));
		AddDisposable(inputBindStruct);
		m_DeclineHint.SetLabel(UIStrings.Instance.CommonTexts.CloseWindow);
		m_NavigationBehaviour.FocusOnEntityManual(m_OpenFolderButton);
		GamePad.Instance.PushLayer(m_InputLayer);
	}

	protected override void Show()
	{
		base.Show();
		m_NavigationBehaviour.FocusOnEntityManual(m_OpenFolderButton);
	}

	protected override void Hide()
	{
		base.Hide();
		m_NavigationBehaviour.Clear();
		GamePad.Instance.PopLayer(m_InputLayer);
	}
}
