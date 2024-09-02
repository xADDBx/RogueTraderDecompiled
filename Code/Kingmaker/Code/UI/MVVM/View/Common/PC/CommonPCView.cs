using System;
using System.Collections;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ChoseControllerMode;
using Kingmaker.Code.UI.MVVM.View.ContextMenu.PC;
using Kingmaker.Code.UI.MVVM.View.CounterWindow;
using Kingmaker.Code.UI.MVVM.View.DlcManager.PC;
using Kingmaker.Code.UI.MVVM.View.EscMenu.PC;
using Kingmaker.Code.UI.MVVM.View.Fade;
using Kingmaker.Code.UI.MVVM.View.MessageBox.PC;
using Kingmaker.Code.UI.MVVM.View.Pause.PC;
using Kingmaker.Code.UI.MVVM.View.Settings.PC;
using Kingmaker.Code.UI.MVVM.View.Titles;
using Kingmaker.Code.UI.MVVM.View.Tooltip.PC;
using Kingmaker.Code.UI.MVVM.View.UIVisibility;
using Kingmaker.Code.UI.MVVM.View.WarningNotification;
using Kingmaker.Code.UI.MVVM.VM.Common;
using Kingmaker.Code.UI.MVVM.VM.DlcManager;
using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Kingmaker.Code.UI.MVVM.VM.Settings;
using Kingmaker.Code.UI.MVVM.VM.UIVisibility;
using Kingmaker.Modding;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.DragNDrop;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.MVVM.View.BugReport.PC;
using Kingmaker.UI.MVVM.View.NetLobby.PC;
using Kingmaker.UI.MVVM.View.NetRoles.PC;
using Kingmaker.UI.MVVM.View.QuestNotification;
using Kingmaker.UI.MVVM.View.SaveLoad.PC;
using Kingmaker.UI.MVVM.View.Tutorial.PC;
using Kingmaker.UI.MVVM.VM.Credits;
using Kingmaker.UI.MVVM.VM.NetLobby;
using Kingmaker.UI.MVVM.VM.NetRoles;
using Kingmaker.UI.Selection;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Common.PC;

public class CommonPCView : ViewBase<CommonVM>, IInitializable
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
	private EscMenuContextPCView m_EscMenuContextPCView;

	[SerializeField]
	private TooltipContextPCView m_TooltipContextPCView;

	[SerializeField]
	private QuestNotificatorPCView m_QuestNotificatorPCView;

	[SerializeField]
	private TutorialPCView m_TutorialPCView;

	[SerializeField]
	private MessageBoxPCView m_MessageBoxPCView;

	[SerializeField]
	private CounterWindowPCView m_CounterWindowPCView;

	[SerializeField]
	private ContextMenuPCView m_ContextMenuPCView;

	[SerializeField]
	private UIDestroyViewLink<SaveLoadPCView, SaveLoadVM> m_SaveLoadPCView;

	[SerializeField]
	private UIDestroyViewLink<SettingsPCView, SettingsVM> m_SettingsPCView;

	[SerializeField]
	private GamepadConnectedInKeyboardModeWindowView m_GamepadConnectedInKeyboardModeWindowView;

	[SerializeField]
	private PauseNotificationPCView m_PauseNotification;

	[SerializeField]
	private WarningsTextView m_WarningsTextView;

	[SerializeField]
	private FadeView m_FadeView;

	[SerializeField]
	private DragNDropManager m_DragNDropManager;

	[SerializeField]
	private MultiplySelection m_MultiplySelection;

	[SerializeField]
	private OwlcatModificationsWindow m_OwlcatModificationsWindow;

	[SerializeField]
	private BugReportPCView m_BugReportView;

	private bool m_IsInit;

	[SerializeField]
	private UIDestroyViewLink<NetLobbyPCView, NetLobbyVM> m_NetLobbyPCView;

	[SerializeField]
	private UIDestroyViewLink<NetRolesPCView, NetRolesVM> m_NetRolesPCView;

	[SerializeField]
	private UIDestroyViewLink<DlcManagerPCView, DlcManagerVM> m_DlcManagerPCView;

	[SerializeField]
	private UIDestroyViewLink<TitlesBaseView, TitlesVM> m_TitlesView;

	private Coroutine m_DisappearAnimationCoroutine;

	private IDisposable m_EscHotkey;

	public void Initialize()
	{
		m_UIVisibilityCommonView.Initialize();
		m_UIVisibilityBugReportView.Initialize();
		m_EscMenuContextPCView.Initialize();
		m_TooltipContextPCView.Initialize();
		m_QuestNotificatorPCView.Initialize();
		m_TutorialPCView.Initialize();
		m_MessageBoxPCView.Initialize();
		m_CounterWindowPCView.Initialize();
		m_ContextMenuPCView.Initialize();
		m_GamepadConnectedInKeyboardModeWindowView.Initialize();
		m_WarningsTextView.Initialize();
		m_FadeView.Initialize();
		m_DragNDropManager.Initialize();
		m_MultiplySelection.Initialize();
		m_BugReportView.Initialize();
		m_UIVisibilityFadeAnimator.Initialize();
	}

	protected override void BindViewImplementation()
	{
		m_UIVisibilityCommonView.Bind(base.ViewModel.UIVisibilityVM);
		m_UIVisibilityBugReportView.Bind(base.ViewModel.UIVisibilityVM);
		m_EscMenuContextPCView.Bind(base.ViewModel.EscMenuContextVM);
		m_TooltipContextPCView.Bind(base.ViewModel.TooltipContextVM);
		m_QuestNotificatorPCView.Bind(base.ViewModel.QuestNotificatorVM);
		m_GamepadConnectedInKeyboardModeWindowView.Bind(base.ViewModel.GamepadConnectDisconnectVM);
		m_WarningsTextView.Bind(base.ViewModel.WarningsTextVM);
		m_FadeView.Bind(base.ViewModel.FadeVM);
		m_TutorialPCView.Bind(base.ViewModel.TutorialVM);
		m_PauseNotification.Bind(base.ViewModel.PauseNotificationVM);
		AddDisposable(base.ViewModel.MessageBoxVM.Subscribe(m_MessageBoxPCView.Bind));
		AddDisposable(base.ViewModel.CounterWindowVM.Subscribe(m_CounterWindowPCView.Bind));
		AddDisposable(base.ViewModel.ContextMenuVM.Subscribe(m_ContextMenuPCView.Bind));
		AddDisposable(base.ViewModel.SaveLoadVM.Subscribe(m_SaveLoadPCView.Bind));
		AddDisposable(base.ViewModel.SettingsVM.Subscribe(m_SettingsPCView.Bind));
		AddDisposable(m_OwlcatModificationsWindow.Bind());
		AddDisposable(base.ViewModel.BugReportVM.Subscribe(m_BugReportView.Bind));
		AddDisposable(base.ViewModel.NetLobbyVM.Subscribe(m_NetLobbyPCView.Bind));
		AddDisposable(base.ViewModel.NetRolesVM.Subscribe(m_NetRolesPCView.Bind));
		AddDisposable(base.ViewModel.DlcManagerVM.Subscribe(m_DlcManagerPCView.Bind));
		AddDisposable(base.ViewModel.TitlesVM.Subscribe(m_TitlesView.Bind));
		AddDisposable(UIVisibilityState.VisibilityPreset.Skip(1).Subscribe(delegate
		{
			UIVisibilityChange();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		m_DragNDropManager.Dispose();
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
