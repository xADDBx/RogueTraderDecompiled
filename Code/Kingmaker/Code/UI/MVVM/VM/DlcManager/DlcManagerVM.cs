using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.DlcManager.Dlcs;
using Kingmaker.Code.UI.MVVM.VM.DlcManager.Mods;
using Kingmaker.Code.UI.MVVM.VM.DlcManager.SwitchOnDlcs;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.DlcManager;

public class DlcManagerVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, INetLobbyRequest, ISubscriber, INetInviteHandler, IBugReportUIHandler
{
	private readonly List<DlcManagerMenuEntityVM> m_MenuEntitiesList = new List<DlcManagerMenuEntityVM>();

	public readonly ReactiveCommand ChangeTab = new ReactiveCommand();

	public readonly SelectionGroupRadioVM<DlcManagerMenuEntityVM> MenuSelectionGroup;

	public readonly ReactiveProperty<DlcManagerMenuEntityVM> SelectedMenuEntity = new ReactiveProperty<DlcManagerMenuEntityVM>();

	private readonly Action m_OnClose;

	private DlcManagerMenuEntityVM m_PreviousSelectedMenuEntity;

	public readonly bool InGame;

	public readonly bool IsConsole;

	public readonly BoolReactiveProperty IsModsWindow = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsSwitchOnDlcsWindow = new BoolReactiveProperty();

	public DlcManagerTabDlcsVM DlcsVM { get; }

	public DlcManagerTabModsVM ModsVM { get; }

	public DlcManagerTabSwitchOnDlcsVM SwitchOnDlcsVM { get; }

	public DlcManagerVM(Action closeAction, bool inGame = false)
	{
		m_OnClose = closeAction;
		InGame = inGame;
		IsConsole = false;
		if (!inGame)
		{
			AddDisposable(DlcsVM = new DlcManagerTabDlcsVM());
		}
		else
		{
			AddDisposable(SwitchOnDlcsVM = new DlcManagerTabSwitchOnDlcsVM());
		}
		if (!IsConsole)
		{
			AddDisposable(ModsVM = new DlcManagerTabModsVM(!inGame));
		}
		if (!inGame)
		{
			CreateMenuEntity(UIStrings.Instance.DlcManager.DlcManagerLabel, DlcsVM, OnDlcsMenuSelect);
		}
		else
		{
			CreateMenuEntity(UIStrings.Instance.DlcManager.DlcManagerLabel, SwitchOnDlcsVM, OnSwitchOnDlcsMenuSelect);
		}
		if (!IsConsole)
		{
			CreateMenuEntity(UIStrings.Instance.DlcManager.ModsLabel, ModsVM, OnModsMenuSelect);
		}
		MenuSelectionGroup = new SelectionGroupRadioVM<DlcManagerMenuEntityVM>(m_MenuEntitiesList, SelectedMenuEntity);
		SelectedMenuEntity.Value = m_MenuEntitiesList.FirstOrDefault();
		AddDisposable(MenuSelectionGroup);
		AddDisposable(EventBus.Subscribe(this));
		if (!InGame)
		{
			SetStoreIconVisibility(visible: true);
		}
	}

	protected override void DisposeImplementation()
	{
		SetStoreIconVisibility(visible: false);
	}

	private void CreateMenuEntity(LocalizedString localizedString, DlcManagerTabBaseVM dlcManagerTab, Action callback)
	{
		DlcManagerMenuEntityVM dlcManagerMenuEntityVM = new DlcManagerMenuEntityVM(localizedString, dlcManagerTab, callback);
		AddDisposable(dlcManagerMenuEntityVM);
		m_MenuEntitiesList.Add(dlcManagerMenuEntityVM);
	}

	private void OnDlcsMenuSelect()
	{
		CheckToChangeTab(delegate
		{
			if (!InGame)
			{
				DlcsVM.SetEnabled(value: true);
			}
			else
			{
				SwitchOnDlcsVM.SetEnabled(value: false);
				IsSwitchOnDlcsWindow.Value = false;
			}
			if (!IsConsole)
			{
				ModsVM.SetEnabled(value: false);
				IsModsWindow.Value = false;
			}
			ChangeTab.Execute();
		});
	}

	private void OnModsMenuSelect()
	{
		CheckToChangeTab(delegate
		{
			if (!InGame)
			{
				DlcsVM.SetEnabled(value: false);
			}
			else
			{
				SwitchOnDlcsVM.SetEnabled(value: false);
				IsSwitchOnDlcsWindow.Value = false;
			}
			if (!IsConsole)
			{
				ModsVM.SetEnabled(value: true);
				IsModsWindow.Value = true;
			}
			ChangeTab.Execute();
		});
	}

	private void OnSwitchOnDlcsMenuSelect()
	{
		CheckToChangeTab(delegate
		{
			if (!InGame)
			{
				DlcsVM.SetEnabled(value: false);
			}
			else
			{
				SwitchOnDlcsVM.SetEnabled(value: true);
				IsSwitchOnDlcsWindow.Value = true;
			}
			if (!IsConsole)
			{
				ModsVM.SetEnabled(value: false);
				IsModsWindow.Value = false;
			}
			ChangeTab.Execute();
		});
	}

	public void OnClose()
	{
		CheckToReloadGame(delegate
		{
			m_OnClose?.Invoke();
		});
	}

	private void CheckToChangeTab(Action closeAction)
	{
		if (m_PreviousSelectedMenuEntity != SelectedMenuEntity.Value)
		{
			m_PreviousSelectedMenuEntity = SelectedMenuEntity.Value;
			CheckToReloadGame(closeAction);
		}
	}

	public void CheckToReloadGame(Action closeAction)
	{
		if (!IsConsole && ModsVM.CheckNeedToReloadGame() && IsModsWindow.Value)
		{
			UIUtility.ShowMessageBox(UIStrings.Instance.DlcManager.RestartChangeModConfirmation, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
			{
				RestartGame(button, closeAction);
			}, null, UIStrings.Instance.DlcManager.RestartGame, UIStrings.Instance.SettingsUI.DialogRevert);
		}
		else if (InGame && SwitchOnDlcsVM.CheckNeedToResaveGame() && IsSwitchOnDlcsWindow.Value)
		{
			UIUtility.ShowMessageBox(UIStrings.Instance.DlcManager.YouSwitchDlcOnAndCantDoItBack, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
			{
				ChangeDlcsStates(button, closeAction);
			}, null, null, UIStrings.Instance.SettingsUI.DialogRevert);
		}
		else
		{
			closeAction?.Invoke();
		}
	}

	public void RestoreAllToPreviousState()
	{
		if (!IsConsole && ModsVM.CheckNeedToReloadGame() && IsModsWindow.Value)
		{
			UIUtility.ShowMessageBox(UIStrings.Instance.DlcManager.ResetAllModsToPreviousState, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
			{
				if (button == DialogMessageBoxBase.BoxButton.Yes && !IsConsole && IsModsWindow.Value)
				{
					ModsVM.ResetModsCurrentState();
				}
			});
		}
		else
		{
			if (!InGame || !SwitchOnDlcsVM.CheckNeedToResaveGame() || !IsSwitchOnDlcsWindow.Value)
			{
				return;
			}
			UIUtility.ShowMessageBox(UIStrings.Instance.DlcManager.ResetAllDlcsToPreviousState, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
			{
				if (button == DialogMessageBoxBase.BoxButton.Yes && InGame && IsSwitchOnDlcsWindow.Value)
				{
					SwitchOnDlcsVM.ResetDlcsCurrentState();
				}
			});
		}
	}

	private void RestartGame(DialogMessageBoxBase.BoxButton button, Action closeBoxAction)
	{
		if (button == DialogMessageBoxBase.BoxButton.No)
		{
			if (!IsConsole)
			{
				ModsVM.ResetModsCurrentState();
			}
			closeBoxAction?.Invoke();
			return;
		}
		ModsVM.SetModsCurrentState();
		if (InGame)
		{
			Game.Instance.MakeQuickSave(delegate
			{
				SystemUtil.ApplicationRestart();
			});
		}
		else
		{
			SystemUtil.ApplicationRestart();
		}
	}

	private void ChangeDlcsStates(DialogMessageBoxBase.BoxButton button, Action closeBoxAction)
	{
		if (!InGame || IsModsWindow.Value || !IsSwitchOnDlcsWindow.Value)
		{
			return;
		}
		if (button == DialogMessageBoxBase.BoxButton.No)
		{
			if (InGame && !IsModsWindow.Value && IsSwitchOnDlcsWindow.Value)
			{
				SwitchOnDlcsVM.ResetDlcsCurrentState();
			}
			closeBoxAction?.Invoke();
		}
		else
		{
			SwitchOnDlcsVM.SetDlcsCurrentState();
			m_OnClose?.Invoke();
		}
	}

	public void HandleNetLobbyRequest(bool isMainMenu = false)
	{
		m_OnClose?.Invoke();
	}

	public void HandleNetLobbyClose()
	{
	}

	public void HandleInvite(Action<bool> callback)
	{
	}

	public void HandleInviteAccepted(bool accepted)
	{
		if (accepted)
		{
			m_OnClose?.Invoke();
		}
	}

	private void SetStoreIconVisibility(bool visible)
	{
	}

	public void HandleBugReportOpen(bool showBugReportOnly)
	{
	}

	public void HandleBugReportCanvasHotKeyOpen()
	{
	}

	public void HandleBugReportShow()
	{
		SetStoreIconVisibility(visible: false);
	}

	public void HandleBugReportHide()
	{
		if (!InGame)
		{
			SetStoreIconVisibility(visible: true);
		}
	}

	public void HandleUIElementFeature(string featureName)
	{
	}
}
