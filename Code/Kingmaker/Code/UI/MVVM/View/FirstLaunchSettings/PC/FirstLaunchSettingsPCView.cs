using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.Base;
using Kingmaker.Code.UI.MVVM.View.Settings.PC.Menu;
using Kingmaker.Localization;
using Kingmaker.UI.Models.SettingsUI;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.FirstLaunchSettings.PC;

public class FirstLaunchSettingsPCView : FirstLaunchSettingsBaseView
{
	[Header("Main")]
	[SerializeField]
	private SettingsMenuSelectorPCView m_MenuSelector;

	[SerializeField]
	private FirstLaunchLanguagePagePCView m_LanguagePagePCView;

	[SerializeField]
	private FirstLaunchDisplayPagePCView m_DisplayPagePCView;

	[SerializeField]
	private FirstLaunchAccessibilityPagePCView m_AccessibilityPagePCView;

	[Header("Buttons")]
	[SerializeField]
	private OwlcatButton m_BackButton;

	[SerializeField]
	private OwlcatButton m_ResetToDefaultButton;

	[SerializeField]
	private OwlcatButton m_ContinueButton;

	[SerializeField]
	private TextMeshProUGUI m_BackText;

	[SerializeField]
	private TextMeshProUGUI m_ResetToDefaultText;

	[SerializeField]
	private TextMeshProUGUI m_ContinueText;

	protected override void InitializeImpl()
	{
		m_MenuSelector.Initialize();
		m_LanguagePagePCView.Initialize();
		m_DisplayPagePCView.Initialize();
		m_AccessibilityPagePCView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_MenuSelector.Bind(base.ViewModel.SelectionGroup);
		AddDisposable(base.ViewModel.LanguagePageVM.Subscribe(m_LanguagePagePCView.Bind));
		AddDisposable(base.ViewModel.DisplayPageVM.Subscribe(m_DisplayPagePCView.Bind));
		AddDisposable(base.ViewModel.AccessiabilityPageVM.Subscribe(m_AccessibilityPagePCView.Bind));
		AddDisposable(IsNotOnLanguagePage.Subscribe(m_BackButton.gameObject.SetActive));
		AddDisposable(IsNotOnLanguagePage.Subscribe(m_ResetToDefaultButton.gameObject.SetActive));
		AddDisposable(m_BackButton.OnLeftClickAsObservable().Subscribe(base.DeclineAction));
		AddDisposable(m_ContinueButton.OnLeftClickAsObservable().Subscribe(base.ConfirmAction));
		AddDisposable(m_ResetToDefaultButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.RevertSettings));
		AddDisposable(m_BackButton.OnConfirmClickAsObservable().Subscribe(base.DeclineAction));
		AddDisposable(m_ContinueButton.OnConfirmClickAsObservable().Subscribe(base.ConfirmAction));
		AddDisposable(m_ResetToDefaultButton.OnConfirmClickAsObservable().Subscribe(base.ViewModel.RevertSettings));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.PrevTab.name, base.DeclineAction));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.NextTab.name, base.ConfirmAction));
	}

	protected override void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		m_LanguagePagePCView.SetNavigationBehaviour(navigationBehaviour);
		m_DisplayPagePCView.SetNavigationBehaviour(navigationBehaviour);
		m_AccessibilityPagePCView.SetNavigationBehaviour(navigationBehaviour);
		IConsoleEntity[] entities = new IConsoleEntity[3] { m_BackButton, m_ResetToDefaultButton, m_ContinueButton };
		m_LanguagePagePCView.AddNavigationEntities(entities);
		m_DisplayPagePCView.AddNavigationEntities(entities);
		m_AccessibilityPagePCView.AddNavigationEntities(entities);
	}

	protected override void SetupTexts()
	{
		LocalizedString localizedString = (IsFocusedOnLanguageItem ? UIStrings.Instance.SettingsUI.Cancel : UIStrings.Instance.ContextMenu.Back);
		LocalizedString localizedString2 = (IsFocusedOnLanguageItem ? UIStrings.Instance.SettingsUI.Apply : UIStrings.Instance.MainMenu.Continue);
		m_ResetToDefaultText.text = UIStrings.Instance.SettingsUI.Default;
		m_BackText.text = localizedString;
		m_ContinueText.text = localizedString2;
	}
}
