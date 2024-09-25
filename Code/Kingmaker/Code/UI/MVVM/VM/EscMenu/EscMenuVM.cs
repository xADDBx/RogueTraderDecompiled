using System;
using System.Text;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.Utility;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.EscMenu;

public class EscMenuVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IVendorUIHandler, ISubscriber<IMechanicEntity>, ISubscriber, IGameModeHandler, INetLobbyRequest
{
	private readonly Action m_CloseAction;

	public readonly BoolReactiveProperty IsInSpace = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsInCutscene = new BoolReactiveProperty();

	public bool InternalWindowOpened;

	public readonly ReactiveCommand UpdateButtonsInteractable = new ReactiveCommand();

	public readonly ReactiveCommand UpdateButtonsFocus = new ReactiveCommand();

	public bool IsSavingAllowed { get; private set; }

	public bool IsOptionsAllowed { get; private set; }

	public bool IsFormationAllowed { get; private set; }

	public EscMenuVM(Action closeAction)
	{
		IsInSpace.Value = Game.Instance.IsModeActive(GameModeType.SpaceCombat) || Game.Instance.IsModeActive(GameModeType.StarSystem) || Game.Instance.IsModeActive(GameModeType.GlobalMap);
		IsInCutscene.Value = Game.Instance.IsModeActive(GameModeType.Cutscene);
		IsOptionsAllowed = !IsInCutscene.Value;
		IsFormationAllowed = !IsInCutscene.Value && !Game.Instance.IsModeActive(GameModeType.Dialog);
		m_CloseAction = closeAction;
		MainThreadDispatcher.StartCoroutine(UIUtilityCheckSaves.WaitForSaveUpdated(delegate
		{
			IsSavingAllowed = Game.Instance.SaveManager.IsSaveAllowed(SaveInfo.SaveType.Manual) && !SettingsRoot.Difficulty.OnlyOneSave;
		}));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	public void OnClose()
	{
		m_CloseAction?.Invoke();
	}

	public void OnSave()
	{
		InternalWindowOpened = true;
		OnClose();
		EventBus.RaiseEvent(delegate(ISaveLoadUIHandler h)
		{
			h.HandleOpenSaveLoad(SaveLoadMode.Save, singleMode: false);
		});
	}

	public void OnLoad()
	{
		InternalWindowOpened = true;
		OnClose();
		EventBus.RaiseEvent(delegate(ISaveLoadUIHandler h)
		{
			h.HandleOpenSaveLoad(SaveLoadMode.Load, !IsSavingAllowed);
		});
	}

	public void OnQuickSave()
	{
		MainThreadDispatcher.StartCoroutine(UIUtilityCheckSaves.WaitForSaveUpdated(delegate
		{
			m_CloseAction?.Invoke();
			Game.Instance.MakeQuickSave();
		}));
	}

	public void OnQuickLoad()
	{
		MainThreadDispatcher.StartCoroutine(UIUtilityCheckSaves.WaitForSaveUpdated(delegate
		{
			OnClose();
			Game.Instance.QuickLoadGame();
		}));
	}

	public void OpenFormation()
	{
		InternalWindowOpened = true;
		OnClose();
		EventBus.RaiseEvent(delegate(IFormationWindowUIHandler h)
		{
			h.HandleOpenFormation();
		});
	}

	public void OnSettings()
	{
		InternalWindowOpened = true;
		OnClose();
		EventBus.RaiseEvent(delegate(ISettingsUIHandler h)
		{
			h.HandleOpenSettings();
		});
	}

	public void OnMods()
	{
		InternalWindowOpened = true;
		OnClose();
		EventBus.RaiseEvent(delegate(IDlcManagerUIHandler h)
		{
			h.HandleOpenDlcManager(inGame: true);
		});
	}

	public void OnBugReport()
	{
		InternalWindowOpened = true;
		OnClose();
		EventBus.RaiseEvent(delegate(IBugReportUIHandler h)
		{
			h.HandleBugReportOpen(IsInCutscene.Value);
		});
	}

	public void OnMainMenu()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(UIStrings.Instance.CommonTexts.QuitToMainMenuLabel);
		if ((bool)SettingsRoot.Difficulty.OnlyOneSave)
		{
			UIUtility.ShowMessageBox(stringBuilder.ToString(), DialogMessageBoxBase.BoxType.Dialog, OnQuitToMainMenuAction);
			return;
		}
		stringBuilder.Append(" ");
		stringBuilder.Append(UIStrings.Instance.CommonTexts.ProgressWillBeLost);
		UIUtility.ShowMessageBox(stringBuilder.ToString(), DialogMessageBoxBase.BoxType.Dialog, OnQuitToMainMenuAction);
	}

	private void OnQuitToMainMenuAction(DialogMessageBoxBase.BoxButton button)
	{
		if (button != DialogMessageBoxBase.BoxButton.Yes)
		{
			return;
		}
		OnClose();
		if ((bool)SettingsRoot.Difficulty.OnlyOneSave)
		{
			MainThreadDispatcher.StartCoroutine(UIUtilityCheckSaves.WaitForSaveUpdated(delegate
			{
				LoadingProcess.Instance.StartLoadingProcess(Game.Instance.SaveManager.SaveRoutine(Game.Instance.SaveManager.GetNextAutoslot(), forceAuto: true), delegate
				{
					Game.Instance.ResetToMainMenu();
				}, LoadingProcessTag.Save);
			}));
		}
		else
		{
			Game.Instance.ResetToMainMenu();
		}
	}

	public void OnQuit()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(UIStrings.Instance.CommonTexts.QuitToDesctopLabel);
		if ((bool)SettingsRoot.Difficulty.OnlyOneSave)
		{
			UIUtility.ShowMessageBox(stringBuilder.ToString(), DialogMessageBoxBase.BoxType.Dialog, OnQuitAction);
			return;
		}
		stringBuilder.Append(" ");
		stringBuilder.Append(UIStrings.Instance.CommonTexts.ProgressWillBeLost);
		UIUtility.ShowMessageBox(stringBuilder.ToString(), DialogMessageBoxBase.BoxType.Dialog, OnQuitAction);
	}

	private void OnQuitAction(DialogMessageBoxBase.BoxButton button)
	{
		SoundState.Instance.MusicStateHandler.StartMusicStopEvent();
		if (button != DialogMessageBoxBase.BoxButton.Yes)
		{
			return;
		}
		if ((bool)SettingsRoot.Difficulty.OnlyOneSave)
		{
			MainThreadDispatcher.StartCoroutine(UIUtilityCheckSaves.WaitForSaveUpdated(delegate
			{
				LoadingProcess.Instance.StartLoadingProcess(Game.Instance.SaveManager.SaveRoutine(Game.Instance.SaveManager.GetNextAutoslot(), forceAuto: true), SystemUtil.ApplicationQuit, LoadingProcessTag.Save);
			}));
		}
		else
		{
			SystemUtil.ApplicationQuit();
		}
	}

	public void OnMultiplayer()
	{
		InternalWindowOpened = true;
		OnClose();
		EventBus.RaiseEvent(delegate(INetLobbyRequest h)
		{
			h.HandleNetLobbyRequest();
		});
	}

	public void OnMultiplayerRoles()
	{
		InternalWindowOpened = true;
		OnClose();
		EventBus.RaiseEvent(delegate(INetRolesRequest h)
		{
			h.HandleNetRolesRequest();
		});
	}

	public void HandleTradeStarted()
	{
		OnClose();
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		IsInSpace.Value = Game.Instance.IsModeActive(GameModeType.SpaceCombat) || Game.Instance.IsModeActive(GameModeType.StarSystem) || Game.Instance.IsModeActive(GameModeType.GlobalMap);
		IsInCutscene.Value = Game.Instance.IsModeActive(GameModeType.Cutscene);
		IsOptionsAllowed = !IsInCutscene.Value;
		IsFormationAllowed = !IsInCutscene.Value && !Game.Instance.IsModeActive(GameModeType.Dialog);
		MainThreadDispatcher.StartCoroutine(UIUtilityCheckSaves.WaitForSaveUpdated(delegate
		{
			IsSavingAllowed = Game.Instance.SaveManager.IsSaveAllowed(SaveInfo.SaveType.Manual) && !SettingsRoot.Difficulty.OnlyOneSave;
		}));
		UpdateButtonsInteractable.Execute();
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void HandleNetLobbyRequest(bool isMainMenu = false)
	{
		InternalWindowOpened = true;
		OnClose();
	}

	public void HandleNetLobbyClose()
	{
	}
}
