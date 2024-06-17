using System;
using System.Text;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.Utility;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.EscMenu;

public class EscMenuVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private readonly Action m_CloseAction;

	public readonly BoolReactiveProperty IsInSpace = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsInCutscene = new BoolReactiveProperty();

	public bool InternalWindowOpened;

	public bool IsSavingAllowed { get; private set; }

	public bool IsOptionsAllowed { get; }

	public bool IsFormationAllowed { get; }

	public EscMenuVM(Action closeAction)
	{
		IsInSpace.Value = Game.Instance.CurrentMode == GameModeType.SpaceCombat || Game.Instance.CurrentMode == GameModeType.StarSystem || Game.Instance.CurrentMode == GameModeType.GlobalMap;
		IsInCutscene.Value = Game.Instance.CurrentMode == GameModeType.Cutscene;
		IsOptionsAllowed = !IsInCutscene.Value;
		IsFormationAllowed = !IsInCutscene.Value && Game.Instance.CurrentMode != GameModeType.Dialog;
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
		m_CloseAction?.Invoke();
		EventBus.RaiseEvent(delegate(ISaveLoadUIHandler h)
		{
			h.HandleOpenSaveLoad(SaveLoadMode.Save, singleMode: false);
		});
	}

	public void OnLoad()
	{
		InternalWindowOpened = true;
		m_CloseAction?.Invoke();
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
			m_CloseAction?.Invoke();
			Game.Instance.QuickLoadGame();
		}));
	}

	public void OpenFormation()
	{
		InternalWindowOpened = true;
		m_CloseAction?.Invoke();
		EventBus.RaiseEvent(delegate(IFormationWindowUIHandler h)
		{
			h.HandleOpenFormation();
		});
	}

	public void OnSettings()
	{
		InternalWindowOpened = true;
		m_CloseAction?.Invoke();
		EventBus.RaiseEvent(delegate(ISettingsUIHandler h)
		{
			h.HandleOpenSettings();
		});
	}

	public void OnBugReport()
	{
		InternalWindowOpened = true;
		m_CloseAction?.Invoke();
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
		m_CloseAction?.Invoke();
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

	private static void OnQuitAction(DialogMessageBoxBase.BoxButton button)
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
		m_CloseAction?.Invoke();
		EventBus.RaiseEvent(delegate(INetLobbyRequest h)
		{
			h.HandleNetLobbyRequest();
		});
	}

	public void OnMultiplayerRoles()
	{
		InternalWindowOpened = true;
		m_CloseAction?.Invoke();
		EventBus.RaiseEvent(delegate(INetRolesRequest h)
		{
			h.HandleNetRolesRequest();
		});
	}
}
