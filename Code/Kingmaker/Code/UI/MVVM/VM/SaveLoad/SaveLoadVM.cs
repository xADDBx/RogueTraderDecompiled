using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.Stores;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SaveLoad;

public class SaveLoadVM : BaseDisposable, ISavesUpdatedHandler, ISubscriber, IViewModel, IBaseDisposable, IDisposable
{
	public readonly SaveLoadMenuVM SaveLoadMenuVM;

	public readonly SaveSlotCollectionVM SaveSlotCollectionVm;

	public NewSaveSlotVM NewSaveSlotVM;

	public readonly ReactiveProperty<SaveSlotVM> SaveFullScreenshot = new ReactiveProperty<SaveSlotVM>();

	public readonly ReactiveProperty<SaveSlotVM> SelectedSaveSlot = new ReactiveProperty<SaveSlotVM>();

	private SelectionGroupRadioVM<SaveSlotVM> m_SelectionGroup;

	private readonly List<SaveSlotVM> m_SaveSlotVMs = new List<SaveSlotVM>();

	private readonly Action m_OnClose;

	private readonly IUILoadService m_LoadService;

	public readonly BoolReactiveProperty SaveListUpdating = new BoolReactiveProperty(initialValue: false);

	public readonly BoolReactiveProperty IsCurrentIronManSave = new BoolReactiveProperty();

	private bool m_JustOpened;

	public IReactiveProperty<SaveLoadMode> Mode { get; } = new ReactiveProperty<SaveLoadMode>();


	private static UISaveLoadTexts SaveLoadTexts => UIStrings.Instance.SaveLoadTexts;

	private bool ShowCorruptionDialog { get; set; }

	public SaveLoadVM(SaveLoadMode mode, bool singleMode, Action onClose, IUILoadService loadService)
	{
		m_OnClose = onClose;
		m_LoadService = loadService;
		Mode.Value = mode;
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(SaveLoadMenuVM = new SaveLoadMenuVM(Mode, singleMode ? new List<SaveLoadMode> { mode } : new List<SaveLoadMode>
		{
			SaveLoadMode.Save,
			SaveLoadMode.Load
		}));
		MainThreadDispatcher.StartCoroutine(UIUtilityCheckSaves.WaitForSaveUpdated(delegate
		{
			SaveInfo saveInfo = Game.Instance.SaveManager.CreateNewSave(UIUtility.DefaultSaveName, extended: true);
			AddDisposable(NewSaveSlotVM = new NewSaveSlotVM(saveInfo, Mode, new SaveLoadActions
			{
				SaveOrLoad = RequestSaveNew,
				ShowScreenshot = RequestShowScreenshot
			}));
			AddDisposable(saveInfo);
		}));
		AddDisposable(SaveSlotCollectionVm = new SaveSlotCollectionVM(Mode, SelectedSaveSlot));
		AddDisposable(Mode.Subscribe(delegate(SaveLoadMode value)
		{
			HideScreenshot();
			NewSaveSlotVM.SetAvailable(value == SaveLoadMode.Save);
			m_SelectionGroup?.TrySelectFirstValidEntity();
		}));
		UpdateSavesCollection();
		StoreManager.OnRefreshDLC += OnRefreshDLC;
	}

	protected override void DisposeImplementation()
	{
		StoreManager.OnRefreshDLC -= OnRefreshDLC;
		if (!Game.Instance.RootUiContext.IsMainMenu)
		{
			SaveScreenshotManager.Instance.Cleanup();
		}
		HideScreenshot();
	}

	private void OnRefreshDLC()
	{
		SaveSlotCollectionVm?.RefreshDLC();
	}

	private void UpdateSavesCollection()
	{
		SaveListUpdating.Value = true;
		Game.Instance.SaveManager.UpdateSaveListAsync();
	}

	private void HandleSaveListUpdate()
	{
		List<SaveInfo> referenceCollection = new List<SaveInfo>(Game.Instance.SaveManager);
		referenceCollection.RemoveAll((SaveInfo s) => Game.Instance.SaveManager.SavesQueuedForDeletion.Contains(s));
		referenceCollection.Sort((SaveInfo s1, SaveInfo s2) => -s1.SystemSaveTime.CompareTo(s2.SystemSaveTime));
		bool allowSwitchOff = Game.Instance.ControllerMode == Game.ControllerModeType.Gamepad;
		ShowCorruptionDialog = false;
		foreach (SaveInfo saveInfo in referenceCollection)
		{
			if (!SaveManager.IsCoopSave(saveInfo) && !SaveManager.IsImportSave(saveInfo) && !m_SaveSlotVMs.Any((SaveSlotVM vm) => vm.ReferenceSaveEquals(saveInfo)))
			{
				SaveSlotVM saveSlotVM = new SaveSlotVM(saveInfo, Mode, new SaveLoadActions
				{
					SaveOrLoad = RequestSaveOrLoad,
					Delete = RequestDeleteSaveInfo,
					ShowScreenshot = RequestShowScreenshot,
					DeleteWithoutBox = DeleteSaveWithoutBox
				}, allowSwitchOff);
				AddDisposable(saveSlotVM);
				SaveSlotCollectionVm.HandleNewSave(saveSlotVM);
				m_SaveSlotVMs.Add(saveSlotVM);
			}
		}
		foreach (SaveSlotVM item in m_SaveSlotVMs.Where((SaveSlotVM saveSlotVm) => !referenceCollection.Any(saveSlotVm.ReferenceSaveEquals)).ToList())
		{
			SaveSlotCollectionVm.HandleDeleteSave(item);
			m_SaveSlotVMs.Remove(item);
		}
		SaveListUpdating.Value = false;
		AddDisposable(m_SelectionGroup = new SelectionGroupRadioVM<SaveSlotVM>(m_SaveSlotVMs, SelectedSaveSlot));
		m_SelectionGroup.InsertEntityAtIndex(0, NewSaveSlotVM);
		if (!m_JustOpened)
		{
			m_SelectionGroup.TrySelectFirstValidEntity();
			m_JustOpened = true;
		}
	}

	private void UpdateSaveSlot(SaveInfo saveInfo)
	{
		m_SaveSlotVMs.FirstOrDefault((SaveSlotVM slot) => slot.ReferenceSaveEquals(saveInfo))?.SetSaveInfo(saveInfo);
	}

	private void RequestSaveNew(SaveInfo saveInfo)
	{
		MainThreadDispatcher.StartCoroutine(UIUtilityCheckSaves.WaitForSaveUpdated(delegate
		{
			SaveManager saveManager = Game.Instance.SaveManager;
			if (saveManager.FirstOrDefault((SaveInfo s) => s.Name.Trim().Equals(saveInfo.Name.Trim(), StringComparison.OrdinalIgnoreCase)) != null)
			{
				int num = 2;
				string name = saveInfo.Name;
				while (saveManager.Any((SaveInfo s) => s.Name.Trim().Equals(saveInfo.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
				{
					saveInfo.Name = $"{name} {num}";
					num++;
				}
			}
			Game.Instance.RequestSaveGame(saveInfo, null, UpdateSavesCollection);
			OnClose();
		}));
	}

	private void RequestSaveOrLoad(SaveInfo saveInfo = null)
	{
		if (saveInfo == null)
		{
			saveInfo = SelectedSaveSlot.Value?.Reference;
		}
		if (saveInfo != null)
		{
			if (Mode.Value == SaveLoadMode.Load)
			{
				RequestLoad(saveInfo);
			}
			else
			{
				RequestOverrideSave(saveInfo);
			}
		}
	}

	private void RequestOverrideSave(SaveInfo saveInfo = null)
	{
		if (saveInfo == null)
		{
			saveInfo = SelectedSaveSlot.Value?.Reference;
		}
		if (saveInfo == null || saveInfo.Type != 0)
		{
			return;
		}
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler h)
		{
			h.HandleOpen(SaveLoadTexts.OverwriteWarning, DialogMessageBoxBase.BoxType.TextField, null, null, inputText: saveInfo.Name, yesLabel: UIStrings.Instance.SettingsUI.DialogSave, noLabel: null, onTextResult: delegate(string text)
			{
				if (!string.IsNullOrEmpty(text))
				{
					ExecuteOverrideSave(saveInfo, text);
					OnClose();
				}
			});
		});
	}

	private void RequestLoad(SaveInfo saveInfo = null)
	{
		SaveInfo si = saveInfo ?? SelectedSaveSlot.Value?.Reference;
		SaveInfo saveInfo2 = si;
		bool flag = saveInfo2 != null && saveInfo2.Type == SaveInfo.SaveType.IronMan;
		bool flag2 = RootUIContext.Instance.IsMainMenu || !SettingsRoot.Difficulty.OnlyOneSave;
		if (PhotonManager.Lobby.IsActive && flag)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.SaveLoadTexts.CannotLoadIronManSaveInCoop, addToLog: false, WarningNotificationFormat.Attention);
			});
			return;
		}
		IsCurrentIronManSave.Value = flag && !RootUIContext.Instance.IsMainMenu && si.GameId == Game.Instance.Player.GameId;
		if (IsCurrentIronManSave.Value)
		{
			UIUtility.ShowMessageBox(UIStrings.Instance.SaveLoadTexts.CannotLoadCurrentIronManSave, DialogMessageBoxBase.BoxType.Message, delegate
			{
			});
			return;
		}
		string text = ((flag && flag2) ? ((string)UIStrings.Instance.SaveLoadTexts.YouLoadIronManSave) : ((!flag && !flag2) ? ((string)UIStrings.Instance.SaveLoadTexts.YouLoadNotIronManSave) : string.Empty));
		if (!string.IsNullOrWhiteSpace(text))
		{
			UIUtility.ShowMessageBox(text, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
			{
				if (button == DialogMessageBoxBase.BoxButton.Yes)
				{
					if ((bool)SettingsRoot.Difficulty.OnlyOneSave)
					{
						MainThreadDispatcher.StartCoroutine(UIUtilityCheckSaves.WaitForSaveUpdated(delegate
						{
							LoadingProcess.Instance.StartLoadingProcess(Game.Instance.SaveManager.SaveRoutine(Game.Instance.SaveManager.GetNextAutoslot(), forceAuto: true), delegate
							{
								m_LoadService.Load(si);
							}, LoadingProcessTag.Save);
						}));
					}
					else
					{
						m_LoadService.Load(si);
					}
				}
			});
		}
		else
		{
			m_LoadService.Load(si);
		}
	}

	private void DeleteSaveWithoutBox(SaveInfo saveInfo = null)
	{
		if (saveInfo == null)
		{
			saveInfo = SelectedSaveSlot.Value?.Reference;
		}
		RequestDeleteSave(saveInfo);
	}

	private void RequestDeleteSaveInfo(SaveInfo saveInfo = null)
	{
		if (saveInfo == null)
		{
			saveInfo = SelectedSaveSlot.Value?.Reference;
		}
		if (saveInfo == null)
		{
			return;
		}
		string deleteWarning = string.Format(SaveLoadTexts.DeleteWarning, saveInfo.Name);
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler h)
		{
			h.HandleOpen(deleteWarning, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton respond)
			{
				if (respond == DialogMessageBoxBase.BoxButton.Yes)
				{
					RequestDeleteSave(saveInfo);
					SaveSlotVM saveSlotVM = m_SaveSlotVMs.FirstOrDefault((SaveSlotVM s) => s.ReferenceSaveEquals(saveInfo));
					if (saveSlotVM != null)
					{
						SaveSlotCollectionVm.HandleDeleteSave(saveSlotVM);
						m_SaveSlotVMs.Remove(saveSlotVM);
					}
				}
			});
		});
	}

	private void RequestShowScreenshot(SaveSlotVM saveSlotVM)
	{
		saveSlotVM?.UpdateHighResScreenshot();
		SaveFullScreenshot.Value = saveSlotVM;
	}

	private void HideScreenshot()
	{
		SaveFullScreenshot.Value = null;
	}

	private void ExecuteOverrideSave(SaveInfo saveInfo, string newName)
	{
		Game.Instance.RequestSaveGame(saveInfo, newName, delegate
		{
			UpdateSaveSlot(saveInfo);
		});
	}

	private void RequestDeleteSave(SaveInfo saveInfo)
	{
		Game.Instance.SaveManager.RequestDeleteSave(saveInfo);
	}

	public void OnClose()
	{
		SaveListUpdating.Value = false;
		IsCurrentIronManSave.Value = false;
		m_JustOpened = false;
		m_OnClose?.Invoke();
	}

	public void OnSaveListUpdated()
	{
		MainThreadDispatcher.StartCoroutine(UIUtilityCheckSaves.WaitForSaveUpdated(delegate
		{
			HandleSaveListUpdate();
		}, waitForDlc: true));
	}
}
