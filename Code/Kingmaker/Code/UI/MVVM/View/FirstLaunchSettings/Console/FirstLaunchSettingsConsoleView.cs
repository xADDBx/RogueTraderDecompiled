using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.Base;
using Kingmaker.Code.UI.MVVM.View.Settings.Console.Menu;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.Console;

public class FirstLaunchSettingsConsoleView : FirstLaunchSettingsBaseView
{
	[Header("Main")]
	[SerializeField]
	private SettingsMenuSelectorConsoleView m_MenuSelector;

	[SerializeField]
	private FirstLaunchLanguagePageConsoleView m_LanguagePageConsoleView;

	[SerializeField]
	private FirstLaunchSafeZonePageConsoleView m_SafeZonePageConsoleView;

	[SerializeField]
	private FirstLaunchDisplayPageConsoleView m_DisplayPageConsoleView;

	[SerializeField]
	private FirstLaunchAccessibilityPageConsoleView m_AccessibilityPageConsoleView;

	[Header("Input")]
	[SerializeField]
	private ConsoleHint m_PrevHint;

	[SerializeField]
	private ConsoleHint m_NextHint;

	[SerializeField]
	private ConsoleHint m_HorizontalDPadHint;

	[SerializeField]
	private ConsoleHint m_VerticalDPadHint;

	[SerializeField]
	protected ConsoleHintsWidget m_ConsoleHintsWidget;

	private IConsoleHint m_BackHint;

	private IConsoleHint m_ContinueHint;

	private IConsoleHint m_FinishHint;

	private IConsoleHint m_ResetToDefaultHint;

	protected override void InitializeImpl()
	{
		m_MenuSelector.Initialize();
		m_LanguagePageConsoleView.Initialize();
		m_SafeZonePageConsoleView.Initialize();
		m_DisplayPageConsoleView.Initialize();
		m_AccessibilityPageConsoleView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_MenuSelector.Bind(base.ViewModel.SelectionGroup);
		AddDisposable(base.ViewModel.LanguagePageVM.Subscribe(m_LanguagePageConsoleView.Bind));
		AddDisposable(base.ViewModel.SafeZonePageVM.Subscribe(m_SafeZonePageConsoleView.Bind));
		AddDisposable(base.ViewModel.DisplayPageVM.Subscribe(m_DisplayPageConsoleView.Bind));
		AddDisposable(base.ViewModel.AccessiabilityPageVM.Subscribe(m_AccessibilityPageConsoleView.Bind));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_ConsoleHintsWidget.Dispose();
	}

	protected override void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		m_LanguagePageConsoleView.SetNavigationBehaviour(navigationBehaviour);
		m_SafeZonePageConsoleView.SetNavigationBehaviour(navigationBehaviour);
		m_DisplayPageConsoleView.SetNavigationBehaviour(navigationBehaviour);
		m_AccessibilityPageConsoleView.SetNavigationBehaviour(navigationBehaviour);
	}

	protected override void CreateInputImpl(InputLayer inputLayer)
	{
		AddDisposable(m_PrevHint.Bind(inputLayer.AddButton(delegate
		{
			m_MenuSelector.OnPrev();
		}, 14, base.ViewModel.BlockHints.Not().ToReactiveProperty())));
		AddDisposable(m_NextHint.Bind(inputLayer.AddButton(delegate
		{
			m_MenuSelector.OnNext();
		}, 15, base.ViewModel.BlockHints.Not().ToReactiveProperty())));
		AddDisposable(m_HorizontalDPadHint.BindCustomAction(21, inputLayer, base.ViewModel.IsVisibleHorizontalDPad));
		AddDisposable(m_VerticalDPadHint.BindCustomAction(22, inputLayer, base.ViewModel.IsVisibleVerticalDPad));
		AddDisposable(m_BackHint = m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			DeclineAction();
		}, 9, IsNotOnLanguagePage.And(base.ViewModel.BlockHints.Not()).ToReactiveProperty())));
		m_BackHint.SetLabel(UIStrings.Instance.ContextMenu.Back);
		AddDisposable(m_ContinueHint = m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			ConfirmAction();
		}, 8, IsVisibleContinueButton.And(base.ViewModel.BlockHints.Not()).ToReactiveProperty())));
		m_ContinueHint.SetLabel(UIStrings.Instance.MainMenu.Continue);
		AddDisposable(m_FinishHint = m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.NextPage();
		}, 8, IsVisibleFinishButton.And(base.ViewModel.BlockHints.Not()).ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed)));
		m_FinishHint.SetLabel(UIStrings.Instance.SettingsUI.FinishSetupHold);
		AddDisposable(m_ResetToDefaultHint = m_ConsoleHintsWidget.BindHint(inputLayer.AddButton(delegate
		{
			base.ViewModel.RevertSettings();
		}, 11, IsNotOnLanguagePage.And(base.ViewModel.BlockHints.Not()).ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed)));
		m_ResetToDefaultHint.SetLabel(UIStrings.Instance.SettingsUI.ResetToDefaultHold);
		AddDisposable(inputLayer.AddAxis(ScrollInfoView, 3, repeat: true));
	}

	private void ScrollInfoView(InputActionEventData arg1, float x)
	{
		if (m_AccessibilityPageConsoleView.gameObject.activeInHierarchy && m_AccessibilityPageConsoleView.IsBinded)
		{
			m_AccessibilityPageConsoleView.ScrollDescription(arg1, x);
		}
		else if (m_DisplayPageConsoleView.gameObject.activeInHierarchy && m_DisplayPageConsoleView.IsBinded)
		{
			m_DisplayPageConsoleView.ScrollDescription(arg1, x);
		}
	}

	protected override void SetupTexts()
	{
		m_BackHint.SetLabel(UIStrings.Instance.ContextMenu.Back);
		m_ContinueHint.SetLabel(UIStrings.Instance.MainMenu.Continue);
		m_FinishHint.SetLabel(UIStrings.Instance.SettingsUI.FinishSetupHold);
		m_ResetToDefaultHint.SetLabel(UIStrings.Instance.SettingsUI.ResetToDefaultHold);
		m_VerticalDPadHint.SetLabel(UIStrings.Instance.SettingsUI.Navigation);
		m_HorizontalDPadHint.SetLabel(UIStrings.Instance.SettingsUI.Value);
	}

	protected override void ShowPhotoSensitivityScreen()
	{
		m_AccessibilityPageConsoleView.ClearNavigationBehaviour();
		base.ShowPhotoSensitivityScreen();
	}
}
