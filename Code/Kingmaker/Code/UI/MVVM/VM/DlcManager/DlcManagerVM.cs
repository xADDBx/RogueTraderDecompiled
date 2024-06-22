using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.DlcManager.Dlcs;
using Kingmaker.Code.UI.MVVM.VM.DlcManager.Mods;
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

public class DlcManagerVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, INetLobbyRequest, ISubscriber, INetInviteHandler
{
	private readonly List<DlcManagerMenuEntityVM> m_MenuEntitiesList = new List<DlcManagerMenuEntityVM>();

	public readonly ReactiveCommand ChangeTab = new ReactiveCommand();

	public readonly SelectionGroupRadioVM<DlcManagerMenuEntityVM> MenuSelectionGroup;

	public readonly ReactiveProperty<DlcManagerMenuEntityVM> SelectedMenuEntity = new ReactiveProperty<DlcManagerMenuEntityVM>();

	private readonly Action m_OnClose;

	private DlcManagerMenuEntityVM m_PreviousSelectedMenuEntity;

	public readonly bool OnlyMods;

	public readonly bool IsConsole;

	public readonly BoolReactiveProperty IsModsWindow = new BoolReactiveProperty();

	public DlcManagerTabDlcsVM DlcsVM { get; }

	public DlcManagerTabModsVM ModsVM { get; }

	public DlcManagerVM(Action closeAction, bool onlyMods = false)
	{
		m_OnClose = closeAction;
		OnlyMods = onlyMods;
		IsConsole = false;
		if (!onlyMods)
		{
			AddDisposable(DlcsVM = new DlcManagerTabDlcsVM());
		}
		if (!IsConsole)
		{
			AddDisposable(ModsVM = new DlcManagerTabModsVM(!onlyMods));
		}
		if (!onlyMods)
		{
			CreateMenuEntity(UIStrings.Instance.DlcManager.DlcManagerLabel, DlcsVM, OnDlcsMenuSelect);
		}
		if (!IsConsole)
		{
			CreateMenuEntity(UIStrings.Instance.DlcManager.ModsLabel, ModsVM, OnModsMenuSelect);
		}
		MenuSelectionGroup = new SelectionGroupRadioVM<DlcManagerMenuEntityVM>(m_MenuEntitiesList, SelectedMenuEntity);
		SelectedMenuEntity.Value = m_MenuEntitiesList.FirstOrDefault();
		AddDisposable(MenuSelectionGroup);
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
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
			if (!OnlyMods)
			{
				DlcsVM.SetEnabled(value: true);
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
			if (!OnlyMods)
			{
				DlcsVM.SetEnabled(value: false);
			}
			if (!IsConsole)
			{
				ModsVM.SetEnabled(value: true);
				IsModsWindow.Value = true;
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
		if (!IsConsole && ModsVM.CheckNeedToReloadGame())
		{
			UIUtility.ShowMessageBox(UIStrings.Instance.DlcManager.RestartChangeModConfirmation, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
			{
				RestartGame(button, closeAction);
			}, null, UIStrings.Instance.DlcManager.RestartGame, UIStrings.Instance.SettingsUI.DialogRevert);
		}
		else
		{
			closeAction?.Invoke();
		}
	}

	public void RestoreAllModsToPreviousState()
	{
		UIUtility.ShowMessageBox(UIStrings.Instance.DlcManager.ResetAllModsToPreviousState, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
		{
			if (button == DialogMessageBoxBase.BoxButton.Yes && !IsConsole)
			{
				ModsVM.ResetModsCurrentState();
			}
		});
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
		if (OnlyMods)
		{
			Game.Instance.MakeQuickSave();
		}
		SystemUtil.ApplicationRestart();
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
}
