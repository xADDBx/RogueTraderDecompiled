using System;
using System.Collections;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.BugReport;
using Kingmaker.Code.UI.MVVM.View.ChoseControllerMode;
using Kingmaker.Code.UI.MVVM.View.ContextMenu.Console;
using Kingmaker.Code.UI.MVVM.View.CounterWindow;
using Kingmaker.Code.UI.MVVM.View.DlcManager.Console;
using Kingmaker.Code.UI.MVVM.View.Fade;
using Kingmaker.Code.UI.MVVM.View.MessageBox.Console;
using Kingmaker.Code.UI.MVVM.View.NetRoles.Console;
using Kingmaker.Code.UI.MVVM.View.Pause.Console;
using Kingmaker.Code.UI.MVVM.View.QuestNotification.Console;
using Kingmaker.Code.UI.MVVM.View.SaveLoad.Console;
using Kingmaker.Code.UI.MVVM.View.Settings.Console;
using Kingmaker.Code.UI.MVVM.View.Titles;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Console;
using Kingmaker.Code.UI.MVVM.View.UIVisibility;
using Kingmaker.Code.UI.MVVM.View.WarningNotification;
using Kingmaker.Code.UI.MVVM.VM.Common;
using Kingmaker.Code.UI.MVVM.VM.DlcManager;
using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Kingmaker.Code.UI.MVVM.VM.Settings;
using Kingmaker.Code.UI.MVVM.VM.UIVisibility;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.MVVM.View.EscMenu.Console;
using Kingmaker.UI.MVVM.View.NetLobby.Console;
using Kingmaker.UI.MVVM.View.Tutorial.Console;
using Kingmaker.UI.MVVM.VM.Credits;
using Kingmaker.UI.MVVM.VM.NetLobby;
using Kingmaker.UI.MVVM.VM.NetRoles;
using Kingmaker.UI.Selection;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Common.Console;

public class CommonConsoleView : ViewBase<CommonVM>, IInitializable
{
	[SerializeField]
	private UIVisibilityView m_UIVisibilityCommonView;

	[SerializeField]
	private UIVisibilityView m_UIVisibilityBugReportView;

	[SerializeField]
	private TextMeshProUGUI m_UIVisibilityText;

	[SerializeField]
	private FadeAnimator m_UIVisibilityFadeAnimator;

	[Space]
	[SerializeField]
	private TutorialConsoleView m_TutorialView;

	[SerializeField]
	private CounterWindowConsoleView m_CounterWindowConsoleView;

	[SerializeField]
	private ContextMenuConsoleView m_ContextMenuConsoleView;

	[SerializeField]
	private TooltipContextConsoleView m_TooltipContextConsoleView;

	[SerializeField]
	private QuestNotificatorConsoleView m_QuestNotificatorConsoleView;

	[SerializeField]
	private GamepadDisconnectedInGamepadModeWindowView m_GamepadDisconnectedInGamepadModeWindowView;

	[SerializeField]
	private PauseNotificationConsoleView m_PauseNotification;

	[SerializeField]
	private WarningsTextView m_WarningsTextView;

	[SerializeField]
	private MultiplySelection m_MultiplySelection;

	[SerializeField]
	private EscMenuContextConsoleView m_EscMenuConsoleView;

	[SerializeField]
	private MessageBoxConsoleView m_MessageBoxConsoleView;

	[SerializeField]
	private UIDestroyViewLink<SaveLoadConsoleView, SaveLoadVM> m_SaveLoadConsoleView;

	[SerializeField]
	private UIDestroyViewLink<SettingsConsoleView, SettingsVM> m_SettingsView;

	[SerializeField]
	private FadeView m_FadeView;

	[SerializeField]
	private BugReportBaseView m_BugReportView;

	[SerializeField]
	private UIDestroyViewLink<NetLobbyConsoleView, NetLobbyVM> m_NetLobbyConsoleView;

	[SerializeField]
	private UIDestroyViewLink<NetRolesConsoleView, NetRolesVM> m_NetRolesConsoleView;

	[SerializeField]
	private UIDestroyViewLink<DlcManagerConsoleView, DlcManagerVM> m_DlcManagerConsoleView;

	[SerializeField]
	private UIDestroyViewLink<TitlesBaseView, TitlesVM> m_TitlesView;

	private Coroutine m_DisappearAnimationCoroutine;

	private IDisposable m_EscHotkey;

	public void Initialize()
	{
		m_UIVisibilityCommonView.Initialize();
		m_UIVisibilityBugReportView.Initialize();
		m_TutorialView.Initialize();
		m_TooltipContextConsoleView.Initialize();
		m_QuestNotificatorConsoleView.Initialize();
		m_GamepadDisconnectedInGamepadModeWindowView.Initialize();
		m_WarningsTextView.Initialize();
		m_MultiplySelection.Initialize();
		m_EscMenuConsoleView.Initialize();
		m_MessageBoxConsoleView.Initialize();
		m_CounterWindowConsoleView.Initialize();
		m_ContextMenuConsoleView.Initialize();
		m_FadeView.Initialize();
		m_BugReportView.Initialize();
		m_UIVisibilityFadeAnimator.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_UIVisibilityCommonView.Bind(base.ViewModel.UIVisibilityVM);
		m_UIVisibilityBugReportView.Bind(base.ViewModel.UIVisibilityVM);
		m_TooltipContextConsoleView.Bind(base.ViewModel.TooltipContextVM);
		m_QuestNotificatorConsoleView.Bind(base.ViewModel.QuestNotificatorVM);
		m_GamepadDisconnectedInGamepadModeWindowView.Bind(base.ViewModel.GamepadConnectDisconnectVM);
		m_EscMenuConsoleView.Bind(base.ViewModel.EscMenuContextVM);
		m_FadeView.Bind(base.ViewModel.FadeVM);
		m_TutorialView.Bind(base.ViewModel.TutorialVM);
		m_WarningsTextView.Bind(base.ViewModel.WarningsTextVM);
		m_PauseNotification.Bind(base.ViewModel.PauseNotificationVM);
		AddDisposable(base.ViewModel.MessageBoxVM.Subscribe(m_MessageBoxConsoleView.Bind));
		AddDisposable(base.ViewModel.SettingsVM.Subscribe(m_SettingsView.Bind));
		AddDisposable(base.ViewModel.ContextMenuVM.Subscribe(m_ContextMenuConsoleView.Bind));
		AddDisposable(base.ViewModel.SaveLoadVM.Subscribe(m_SaveLoadConsoleView.Bind));
		AddDisposable(base.ViewModel.BugReportVM.Subscribe(m_BugReportView.Bind));
		AddDisposable(base.ViewModel.CounterWindowVM.Subscribe(m_CounterWindowConsoleView.Bind));
		AddDisposable(base.ViewModel.NetLobbyVM.Subscribe(m_NetLobbyConsoleView.Bind));
		AddDisposable(base.ViewModel.NetRolesVM.Subscribe(m_NetRolesConsoleView.Bind));
		AddDisposable(base.ViewModel.DlcManagerVM.Subscribe(m_DlcManagerConsoleView.Bind));
		AddDisposable(base.ViewModel.TitlesVM.Subscribe(m_TitlesView.Bind));
		AddDisposable(UIVisibilityState.VisibilityPreset.Skip(1).Subscribe(delegate
		{
			UIVisibilityChange();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		if (m_DisappearAnimationCoroutine != null)
		{
			StopCoroutine(m_DisappearAnimationCoroutine);
			m_DisappearAnimationCoroutine = null;
		}
		m_EscHotkey?.Dispose();
		m_EscHotkey = null;
	}

	private void UIVisibilityChange()
	{
		if (UIVisibilityState.VisibilityPresetIndex != 9)
		{
			if (m_EscHotkey == null)
			{
				m_EscHotkey = EscHotkeyManager.Instance.Subscribe(ResetUIVisibility);
			}
		}
		else
		{
			m_EscHotkey?.Dispose();
			m_EscHotkey = null;
		}
		m_UIVisibilityFadeAnimator.AppearAnimation();
		if (m_DisappearAnimationCoroutine != null)
		{
			StopCoroutine(m_DisappearAnimationCoroutine);
		}
		m_DisappearAnimationCoroutine = StartCoroutine(DisappearAnimationCoroutine());
		string text = UIStrings.Instance.CommonTexts.UIVisibility.Text;
		m_UIVisibilityText.text = text + "<br>" + UIVisibilityState.VisibilityPresetIndex + "/" + 9;
	}

	private void ResetUIVisibility()
	{
		UIVisibilityState.ShowAllUI();
	}

	private IEnumerator DisappearAnimationCoroutine()
	{
		yield return new WaitForSecondsRealtime(1f);
		m_UIVisibilityFadeAnimator.DisappearAnimation();
		m_DisappearAnimationCoroutine = null;
	}
}
