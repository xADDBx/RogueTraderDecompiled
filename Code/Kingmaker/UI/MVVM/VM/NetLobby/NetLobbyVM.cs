using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Common.Dropdown;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.DLC;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.Networking.Platforms;
using Kingmaker.Networking.Platforms.Session;
using Kingmaker.Networking.Player;
using Kingmaker.Networking.Save;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.NetLobby.DlcList;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Fsm;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using Photon.Realtime;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.NetLobby;

public class NetLobbyVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, INetEvents, ISubscriber, ISavesUpdatedHandler, IAreaHandler, INetLobbyPlayersHandler, INetSaveSelectHandler, INetLobbyEpicGamesEvents, INetCheckUsersModsHandler, INetLobbyRequest, StateMachine<NetGame.State, NetGame.Trigger>.IStateMachineEventsHandler
{
	private const string NET_LOBBY_TUTORIAL_PREF_KEY = "first_open_net_lobby_tutorial";

	private const string NET_LOBBY_LAST_SELECTED_JOINABLE_USER_PREF_KEY = "net_lobby_selected_joinable_user";

	private const string NET_LOBBY_LAST_SELECTED_INVITABLE_USER_PREF_KEY = "net_lobby_selected_invitable_user";

	private readonly Action m_CloseAction;

	private string m_Code = string.Empty;

	public readonly BoolReactiveProperty HasCodeForLobby = new BoolReactiveProperty(initialValue: false);

	public readonly ReactiveProperty<OwlcatDropdownVM> RegionDropdownVM = new ReactiveProperty<OwlcatDropdownVM>(null);

	public readonly BoolReactiveProperty IsHost = new BoolReactiveProperty(initialValue: false);

	public readonly BoolReactiveProperty IsInRoom = new BoolReactiveProperty(initialValue: false);

	public readonly BoolReactiveProperty ReadyToHostOrJoin = new BoolReactiveProperty(initialValue: false);

	public readonly BoolReactiveProperty IsSaveAllowed = new BoolReactiveProperty(initialValue: false);

	public readonly BoolReactiveProperty IsSaveTransfer = new BoolReactiveProperty(initialValue: false);

	public readonly IntReactiveProperty SaveTransferProgress = new IntReactiveProperty(0);

	public readonly IntReactiveProperty SaveTransferTarget = new IntReactiveProperty(1);

	public readonly StringReactiveProperty CurrentRegion = new StringReactiveProperty(string.Empty);

	public readonly StringReactiveProperty LobbyCode = new StringReactiveProperty(string.Empty);

	public readonly StringReactiveProperty Version = new StringReactiveProperty(string.Empty);

	public readonly ReactiveProperty<NetGame.State> NetGameCurrentState = new ReactiveProperty<NetGame.State>(NetGame.State.PlatformNotInitialized);

	public readonly ReactiveProperty<SaveSlotVM> CurrentSave = new ReactiveProperty<SaveSlotVM>(null);

	public readonly BoolReactiveProperty NeedReconnect = new BoolReactiveProperty(initialValue: false);

	public readonly List<NetLobbyPlayerVM> PlayerVms = new List<NetLobbyPlayerVM>();

	public readonly ReactiveProperty<SaveSlotCollectionVM> SaveSlotCollectionVm = new ReactiveProperty<SaveSlotCollectionVM>();

	public readonly BoolReactiveProperty ShowWaitingSaveAnim = new BoolReactiveProperty(initialValue: false);

	public readonly BoolReactiveProperty SaveListAreEmpty = new BoolReactiveProperty(initialValue: false);

	public readonly BoolReactiveProperty CanConfirmLaunch = new BoolReactiveProperty();

	public readonly ReactiveProperty<SaveSlotVM> SaveFullScreenshot = new ReactiveProperty<SaveSlotVM>();

	private readonly List<SaveSlotVM> m_SaveSlotVMs = new List<SaveSlotVM>();

	private bool m_NeedUpdateRegion = true;

	public readonly ReactiveProperty<NetLobbyTutorialPartVM> NetLobbyTutorialPartVM = new ReactiveProperty<NetLobbyTutorialPartVM>();

	public readonly ReactiveProperty<NetLobbyDlcListVM> DlcListVM = new ReactiveProperty<NetLobbyDlcListVM>();

	private readonly Dictionary<string, List<IBlueprintDlc>> m_DifferentDlcWithSaveProblems = new Dictionary<string, List<IBlueprintDlc>>();

	public readonly Dictionary<string, List<IBlueprintDlc>> ProblemsToShowInSaveList = new Dictionary<string, List<IBlueprintDlc>>();

	public readonly ReactiveCommand<Dictionary<string, List<IBlueprintDlc>>> CheckProblemsWithDlcs = new ReactiveCommand<Dictionary<string, List<IBlueprintDlc>>>();

	public readonly BoolReactiveProperty NetLobbyTutorialOnScreen = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsAnyTutorialBlocks = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsEnoughPlayersForGame = new BoolReactiveProperty();

	private NetLobbyErrorHandler m_NetLobbyErrorHandler;

	public readonly BoolReactiveProperty IsPlayingState = new BoolReactiveProperty();

	public readonly BoolReactiveProperty EpicGamesButtonActive = new BoolReactiveProperty();

	public readonly BoolReactiveProperty EpicGamesAuthorized = new BoolReactiveProperty();

	public readonly StringReactiveProperty EpicGamesUserName = new StringReactiveProperty();

	public readonly ReactiveProperty<NetLobbyInvitePlayerDifferentPlatformsVM> DifferentPlatformInviteVM = new ReactiveProperty<NetLobbyInvitePlayerDifferentPlatformsVM>();

	public readonly StringReactiveProperty PlayersDifferentMods = new StringReactiveProperty();

	public OwlcatDropdownVM JoinableUserTypesDropdownVM;

	public OwlcatDropdownVM InvitableUserTypesDropdownVM;

	public readonly ReactiveProperty<JoinableUserTypes> CurrentJoinableUserType = new ReactiveProperty<JoinableUserTypes>();

	public readonly ReactiveProperty<InvitableUserTypes> CurrentInvitableUserType = new ReactiveProperty<InvitableUserTypes>();

	private static bool NetLobbyTutorialHasShown => PlayerPrefs.GetInt("first_open_net_lobby_tutorial", 0) == 1;

	private IReactiveProperty<SaveLoadMode> SaveLoadMode { get; } = new ReactiveProperty<SaveLoadMode>(Kingmaker.Code.UI.MVVM.VM.SaveLoad.SaveLoadMode.Load);


	public bool IsConnectingNetGameCurrentState
	{
		get
		{
			NetGame.State value = NetGameCurrentState.Value;
			return value == NetGame.State.PlatformInitializing || value == NetGame.State.NetInitializing || value == NetGame.State.ChangingRegion || value == NetGame.State.CreatingLobby || value == NetGame.State.JoiningLobby;
		}
	}

	public bool IsMainMenu => Game.Instance.SceneLoader.LoadedUIScene == GameScenes.MainMenu;

	public NetLobbyVM(Action closeAction)
	{
		AddDisposable(EventBus.Subscribe(this));
		ActivateNetHandlers();
		m_CloseAction = closeAction;
		IsAnyTutorialBlocks.Value = UIConfig.Instance.BlueprintUINetLobbyTutorial.TutorialBlocksInfo.Any();
		IsSaveAllowed.Value = !LoadingProcess.Instance.IsLoadingInProcess && Game.Instance.SaveManager.IsSaveAllowed(SaveInfo.SaveType.Manual, IsMainMenu);
		PhotonManager.NetGame.StartNetGameIfNeeded();
		for (int i = 0; i < 6; i++)
		{
			PlayerVms.Add(new NetLobbyPlayerVM(DifferentPlatformInviteVM, EpicGamesAuthorized));
		}
		NetGameCurrentState.Value = PhotonManager.NetGame.CurrentState;
		IsPlayingState.Value = NetGameCurrentState.Value == NetGame.State.Playing;
		Version.Value = PhotonManager.Version;
		UpdateRoom();
		AddDisposable(IsInRoom.Subscribe(delegate(bool value)
		{
			IsHost.Value = value && PhotonManager.Instance.IsRoomOwner;
			if (value)
			{
				NetRoomNameHelper.TryFormatString(PhotonManager.Instance.Region, PhotonManager.Instance.RoomName, out var output);
				LobbyCode.Value = output;
				CurrentRegion.Value = PhotonManager.Instance.Region;
				NeedReconnect.Value = PhotonManager.Sync.HasDesync;
				IsSaveAllowed.Value = !LoadingProcess.Instance.IsLoadingInProcess && Game.Instance.SaveManager.IsSaveAllowed(SaveInfo.SaveType.Manual, IsMainMenu);
				if (!PhotonManager.Instance.IsRoomOwner)
				{
					CurrentSave.Value?.Dispose();
					if (PhotonManager.Instance.GetRoomProperty<SaveInfoShort>("si", out var obj) && !obj.IsEmpty)
					{
						SaveInfo saveInfo = (SaveInfo)obj;
						CurrentSave.Value = new SaveSlotVM(saveInfo, new ReactiveProperty<SaveLoadMode>());
					}
				}
				SetPlayers();
			}
			else
			{
				PlayerVms.ForEach(delegate(NetLobbyPlayerVM vm)
				{
					vm.ClearPlayer();
				});
				ResetCurrentSave();
			}
		}));
		AddDisposable(IsHost.Subscribe(delegate
		{
			ResetCurrentSave();
		}));
		AddDisposable(NetGameCurrentState.Subscribe(delegate
		{
			UpdateRoom();
		}));
		AddDisposable(CurrentSave.Subscribe(delegate
		{
			DisposeSaveSloCollection();
		}));
		AddDisposable(SaveSlotCollectionVm.Subscribe(delegate
		{
			m_SaveSlotVMs.ForEach(delegate(SaveSlotVM vm)
			{
				vm.Dispose();
			});
			m_SaveSlotVMs.Clear();
		}));
		AddDisposable(ObservableExtensions.Subscribe(MainThreadDispatcher.UpdateAsObservable(), delegate
		{
			OnUpdate();
		}));
		if (!NetLobbyTutorialHasShown && IsAnyTutorialBlocks.Value)
		{
			ShowNetLobbyTutorial();
		}
		if (PlatformServices.Platform.HasSecondaryPlatform)
		{
			EpicGamesButtonActive.Value = PlatformServices.Platform.HasSecondaryPlatform;
			Platform secondaryPlatform = PlatformServices.Platform.SecondaryPlatform;
			HandleSetEpicGamesUserName(secondaryPlatform.IsInitialized(), secondaryPlatform.User.NickName);
		}
		PhotonManager.IdleConnection.LobbyViewOpened = true;
		SetUserTypesDropdowns();
	}

	protected override void DisposeImplementation()
	{
		PFLog.Net.Log("NET LOBBY STATE DISPOSE");
		HideScreenshot();
		DisposeSaveSloCollection();
		CurrentSave.Value?.Dispose();
		CurrentSave.Value = null;
		PlayerVms.ForEach(delegate(NetLobbyPlayerVM vm)
		{
			vm.Dispose();
		});
		PlayerVms.Clear();
		m_SaveSlotVMs.ForEach(delegate(SaveSlotVM vm)
		{
			vm.Dispose();
		});
		m_SaveSlotVMs.Clear();
		RegionDropdownVM.Value?.Dispose();
		RegionDropdownVM.Value = null;
		m_DifferentDlcWithSaveProblems?.Clear();
		DeactivateNetHandlers();
		PhotonManager.IdleConnection.LobbyViewOpened = false;
	}

	private void SetUserTypesDropdowns()
	{
	}

	private void SetUserTypeDropdown<TEnum>(string playerPrefName, Func<TEnum> getter, Action<TEnum> setter, out OwlcatDropdownVM dropdownVM, IEnumerable<TEnum> options, ReactiveProperty<TEnum> property, Func<TEnum, string> labelGetter, TEnum defaultValue)
	{
		int @int = PlayerPrefs.GetInt(playerPrefName, 0);
		setter(Enum.IsDefined(typeof(TEnum), @int) ? ((TEnum)(object)@int) : defaultValue);
		property.Value = getter();
		List<DropdownItemVM> vmCollection = options.Select((TEnum type) => new DropdownItemVM(labelGetter(type))).ToList();
		AddDisposable(dropdownVM = new OwlcatDropdownVM(vmCollection));
		AddDisposable(property.Subscribe(setter));
	}

	public void SetJoinableUserType(int type)
	{
		CurrentJoinableUserType.Value = (JoinableUserTypes)type;
	}

	public void SetInvitableUserType(int type)
	{
		CurrentInvitableUserType.Value = (InvitableUserTypes)type;
	}

	private void ActivateNetHandlers()
	{
		m_NetLobbyErrorHandler = new NetLobbyErrorHandler();
	}

	private void DeactivateNetHandlers()
	{
		m_NetLobbyErrorHandler.Dispose();
		m_NetLobbyErrorHandler = null;
	}

	private void DisposeSaveSloCollection()
	{
		SaveSlotCollectionVm.Value?.Dispose();
		SaveSlotCollectionVm.Value = null;
		ShowWaitingSaveAnim.Value = false;
		SaveListAreEmpty.Value = false;
	}

	private void SetRegions(RegionHandler regionHandler)
	{
		RegionDropdownVM.Value?.Dispose();
		RegionDropdownVM.Value = null;
		List<Region> enabledRegions = regionHandler.EnabledRegions;
		if (enabledRegions == null || enabledRegions.Count == 0)
		{
			return;
		}
		List<DropdownItemVM> list = new List<DropdownItemVM>();
		int index = 0;
		for (int i = 0; i < enabledRegions.Count; i++)
		{
			Region region = enabledRegions[i];
			list.Add(new NetLobbyRegionDropdownVM($"{region.Code} - {region.Ping}ms", region.Code));
			if (PhotonManager.Instance.Region == region.Code)
			{
				index = i;
			}
		}
		RegionDropdownVM.Value = new OwlcatDropdownVM(list, index);
		AddDisposable(RegionDropdownVM.Value.SelectedVM.Subscribe(delegate(IViewModel itemVM)
		{
			NetLobbyRegionDropdownVM regionVM = itemVM as NetLobbyRegionDropdownVM;
			if (regionVM != null)
			{
				DelayedInvoker.InvokeInFrames(delegate
				{
					PhotonManager.NetGame.ChangeRegion(regionVM.Region);
				}, 1);
			}
		}));
	}

	private void OnUpdate()
	{
		ReadyToHostOrJoin.Value = PhotonManager.ReadyToHostOrJoin;
		if (m_NeedUpdateRegion && PhotonManager.Initialized && PhotonManager.Instance.RegionHandler != null && !PhotonManager.Instance.Region.IsNullOrEmpty())
		{
			SetRegions(PhotonManager.Instance.RegionHandler);
			m_NeedUpdateRegion = false;
		}
		SaveTransferUpdate();
	}

	private void UpdateRoom()
	{
		if (!(PhotonManager.Instance == null))
		{
			IsInRoom.Value = PhotonManager.Instance.InRoom;
			IsHost.Value = PhotonManager.Instance.IsRoomOwner;
			DisposeSaveSloCollection();
		}
	}

	public void OnClose()
	{
		if (SaveSlotCollectionVm.Value != null)
		{
			HideScreenshot();
			DisposeSaveSloCollection();
		}
		else if (PhotonManager.NetGame.CurrentState == NetGame.State.InLobby)
		{
			UIUtility.ShowMessageBox(UIStrings.Instance.NetLobbyTexts.LeaveLobbyMessageBox, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
			{
				if (button == DialogMessageBoxBase.BoxButton.Yes)
				{
					Disconnect("OnClose");
					m_CloseAction?.Invoke();
				}
			});
		}
		else
		{
			m_CloseAction?.Invoke();
		}
	}

	public void CreateLobby()
	{
		PlayerPrefs.SetInt("net_lobby_selected_invitable_user", (int)PhotonManager.NetGame.CurrentInvitableUserType);
		PlayerPrefs.SetInt("net_lobby_selected_joinable_user", (int)PhotonManager.NetGame.CurrentJoinableUserType);
		PhotonManager.NetGame.CreateNewLobby();
	}

	public void JoinLobby()
	{
		NetRoomNameHelper.TryParse(m_Code, PhotonManager.Instance.RegionHandler.EnabledRegions, out var server, out var room);
		PhotonManager.Invite.AcceptInvite(server, room);
	}

	public void StopWaiting()
	{
		Disconnect("StopWaiting");
	}

	public void Disconnect(string reason)
	{
		PhotonManager.Instance.StopPlaying(reason);
	}

	public bool Launch()
	{
		if (CurrentSave.Value?.Reference != null)
		{
			bool num = CurrentSave.Value.Reference.CheckDlcAvailable();
			bool isDlcsInLobbyReady = PhotonManager.DLC.IsDLCsInLobbyReady;
			if (!num || !isDlcsInLobbyReady)
			{
				EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
				{
					h.HandleWarning((!isDlcsInLobbyReady) ? UIStrings.Instance.SaveLoadTexts.DlcListIsNotLoading : UIStrings.Instance.SaveLoadTexts.DlcRequired, addToLog: false, WarningNotificationFormat.Attention);
				});
				return false;
			}
			bool flag = PhotonManager.NetGame.StartGame((SaveInfoKey)CurrentSave.Value.Reference);
			if (flag)
			{
				UISounds.Instance.Sounds.Buttons.FinishChargenButtonClick.Play();
			}
			CanConfirmLaunch.Value = flag;
			return flag;
		}
		if ((PhotonManager.Sync.HasDesync || IsSaveAllowed.Value || CurrentSave.Value?.Reference != null) && !IsMainMenu)
		{
			if ((bool)SettingsRoot.Difficulty.OnlyOneSave)
			{
				EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
				{
					h.HandleWarning(UIStrings.Instance.SaveLoadTexts.CannotLoadIronManSaveInCoop, addToLog: false, WarningNotificationFormat.Attention);
				});
				return false;
			}
			bool num2 = Game.Instance.Player.CheckDlcAvailable();
			bool isDlcsInLobbyReady = PhotonManager.DLC.IsDLCsInLobbyReady;
			bool flag2 = m_DifferentDlcWithSaveProblems.Values.Any((List<IBlueprintDlc> v) => Game.Instance.Player.DlcRewardsToSave.Any((BlueprintDlcReward dr) => dr.Dlcs.Any(v.Contains)));
			if (!num2 || !isDlcsInLobbyReady || flag2)
			{
				EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
				{
					h.HandleWarning((!isDlcsInLobbyReady) ? UIStrings.Instance.SaveLoadTexts.DlcListIsNotLoading : UIStrings.Instance.SaveLoadTexts.DlcRequired, addToLog: false, WarningNotificationFormat.Attention);
				});
				return false;
			}
			bool flag3 = PhotonManager.NetGame.StartGameWithoutSave();
			CanConfirmLaunch.Value = flag3;
			return flag3;
		}
		ChooseSave();
		CanConfirmLaunch.Value = false;
		return false;
	}

	public void SetLobbyCode(string code)
	{
		HasCodeForLobby.Value = PhotonManager.Initialized && PhotonManager.Instance.RegionHandler != null && NetRoomNameHelper.Check(code, PhotonManager.Instance.RegionHandler.EnabledRegions);
		m_Code = code;
	}

	public void CopyLobbyId()
	{
		NetRoomNameHelper.TryFormatString(PhotonManager.Instance.Region, PhotonManager.Instance.RoomName, out var output);
		GUIUtility.systemCopyBuffer = output;
	}

	public string GetCopiedLobbyId()
	{
		return GUIUtility.systemCopyBuffer;
	}

	public void ChooseSave()
	{
		SaveSlotCollectionVm.Value = new SaveSlotCollectionVM(SaveLoadMode, CurrentSave, OnClose);
		Game.Instance.SaveManager.UpdateSaveListAsync();
		ShowWaitingSaveAnim.Value = true;
	}

	public void ResetCurrentSave()
	{
		CurrentSave.Value?.Dispose();
		CurrentSave.Value = null;
		if (PhotonManager.Instance != null && PhotonManager.Instance.IsRoomOwner)
		{
			PhotonManager.Save.SelectSave(null);
		}
	}

	public void HandleTransferProgressChanged(bool value)
	{
		IsSaveTransfer.Value = value;
		SaveTransferUpdate();
	}

	private void SaveTransferUpdate()
	{
		if (IsSaveTransfer.Value && PhotonManager.Save.GetSentProgress(out var progress, out var target))
		{
			SaveTransferProgress.Value = progress;
			SaveTransferTarget.Value = target;
		}
	}

	private void SetPlayers()
	{
		ReadonlyList<PlayerInfo> allPlayers = PhotonManager.Instance.AllPlayers;
		for (int i = 0; i < PlayerVms.Count; i++)
		{
			if (i < allPlayers.Count)
			{
				PlayerVms[i].SetPlayer(allPlayers[i].Player, allPlayers[i].UserId, allPlayers[i].IsActive);
			}
			else
			{
				PlayerVms[i].ClearPlayer();
			}
		}
		CheckEnoughPlayers();
	}

	public void HandleNetStateChanged(LobbyNetManager.State state)
	{
	}

	public void HandleNetGameStateChanged(NetGame.State state)
	{
		NetGameCurrentState.SetValueAndForceNotify(state);
		IsPlayingState.Value = state == NetGame.State.Playing;
		PFLog.Net.Log($"NET LOBBY STATE CHANGE: {state}");
		if (state == NetGame.State.InLobby)
		{
			CanConfirmLaunch.Value = false;
		}
	}

	public void HandleNLoadingScreenClosed()
	{
		CanConfirmLaunch.Value = false;
	}

	public void OnFireTrigger(NetGame.Trigger trigger)
	{
	}

	public void OnStateChanged(NetGame.State oldState, NetGame.State newState)
	{
		PFLog.Net.Log($"NET LOBBY STATE CHANGE: {oldState} -> {newState}");
		if (newState == NetGame.State.InLobby)
		{
			CanConfirmLaunch.Value = false;
		}
	}

	public void OnProcessTrigger(NetGame.Trigger trigger, NetGame.State currentState, NetGame.State nextState)
	{
	}

	public void OnFireException(Exception exception)
	{
	}

	public void OnUnhandledTransition(NetGame.Trigger trigger, NetGame.State currentState)
	{
	}

	public void OnIgnoreTrigger(NetGame.Trigger trigger, NetGame.State currentState)
	{
	}

	public void OnSaveListUpdated()
	{
		if (SaveSlotCollectionVm.Value == null)
		{
			return;
		}
		MainThreadDispatcher.StartCoroutine(UIUtilityCheckSaves.WaitForSaveUpdated(delegate
		{
			ShowWaitingSaveAnim.Value = false;
			ProblemsToShowInSaveList.Clear();
			List<SaveInfo> referenceCollection = new List<SaveInfo>(Game.Instance.SaveManager);
			referenceCollection.RemoveAll(SaveManager.IsCoopSave);
			referenceCollection.RemoveAll((SaveInfo si) => si != null && si.Type == SaveInfo.SaveType.IronMan);
			if (m_DifferentDlcWithSaveProblems.Any())
			{
				List<SaveInfo> source = referenceCollection.Where((SaveInfo s) => s.DlcRewards != null && s.DlcRewards.Any()).ToList();
				IEnumerable<SaveInfo> savesToHide = source.Where((SaveInfo s) => s.GetRequiredDLCMap().Contains((List<IBlueprintDlc> requiredDLCList) => requiredDLCList.Contains(delegate(IBlueprintDlc requiredDLC)
				{
					IEnumerable<KeyValuePair<string, List<IBlueprintDlc>>> enumerable = m_DifferentDlcWithSaveProblems.Where((KeyValuePair<string, List<IBlueprintDlc>> i) => i.Value.Contains(requiredDLC) || requiredDLC.Rewards.Any(delegate(IBlueprintDlcReward r)
					{
						BlueprintDlcReward reward = r as BlueprintDlcReward;
						return reward != null && i.Value.Any((IBlueprintDlc item) => reward.Dlcs.Contains(item));
					}));
					if (!enumerable.Any())
					{
						return false;
					}
					foreach (KeyValuePair<string, List<IBlueprintDlc>> pd in enumerable)
					{
						if (!ProblemsToShowInSaveList.ContainsKey(pd.Key))
						{
							ProblemsToShowInSaveList[pd.Key] = pd.Value;
						}
						else
						{
							foreach (IBlueprintDlc item in pd.Value.Where((IBlueprintDlc value) => !ProblemsToShowInSaveList[pd.Key].Contains(value)))
							{
								ProblemsToShowInSaveList[pd.Key].Add(item);
							}
						}
					}
					return true;
				})));
				referenceCollection.RemoveAll((SaveInfo r) => savesToHide.Contains(r));
			}
			referenceCollection.Sort((SaveInfo s1, SaveInfo s2) => -s1.SystemSaveTime.CompareTo(s2.SystemSaveTime));
			bool allowSwitchOff = Game.Instance.ControllerMode == Game.ControllerModeType.Gamepad;
			foreach (SaveInfo saveInfo in referenceCollection)
			{
				if (!m_SaveSlotVMs.Any((SaveSlotVM vm) => vm.ReferenceSaveEquals(saveInfo)))
				{
					SaveSlotVM saveSlotVM = new SaveSlotVM(saveInfo, SaveLoadMode, new SaveLoadActions
					{
						Select = delegate(SaveInfo info)
						{
							if (!allowSwitchOff)
							{
								SetSelectSaveAction(info);
							}
						},
						SaveOrLoad = delegate(SaveInfo info)
						{
							if (allowSwitchOff)
							{
								SetSelectSaveAction(info);
							}
						},
						ShowScreenshot = RequestShowScreenshot
					}, allowSwitchOff);
					AddDisposable(saveSlotVM);
					SaveSlotCollectionVm.Value.HandleNewSave(saveSlotVM);
					m_SaveSlotVMs.Add(saveSlotVM);
				}
			}
			List<SaveSlotVM> list = new List<SaveSlotVM>();
			foreach (SaveSlotVM item2 in m_SaveSlotVMs.Where((SaveSlotVM saveSlotVm) => !referenceCollection.Any(saveSlotVm.ReferenceSaveEquals)))
			{
				item2.Dispose();
				list.Add(item2);
			}
			foreach (SaveSlotVM item3 in list)
			{
				SaveSlotCollectionVm.Value.HandleDeleteSave(item3);
				m_SaveSlotVMs.Remove(item3);
			}
			SaveListAreEmpty.Value = m_SaveSlotVMs.Count == 0;
		}));
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

	private void SetSelectSaveAction(SaveInfo saveInfo)
	{
		bool num = saveInfo != null && saveInfo.Type == SaveInfo.SaveType.IronMan;
		if (IsMainMenu)
		{
			_ = 1;
		}
		else
			_ = !SettingsRoot.Difficulty.OnlyOneSave;
		if (num)
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.SaveLoadTexts.CannotLoadIronManSaveInCoop, addToLog: false, WarningNotificationFormat.Attention);
			});
		}
		else
		{
			SetSave(saveInfo);
		}
	}

	private void SetSave(SaveInfo saveInfo)
	{
		SaveSlotVM saveSlotVM = new SaveSlotVM(saveInfo, new ReactiveProperty<SaveLoadMode>());
		if (saveSlotVM.Reference != null && !saveSlotVM.Reference.CheckDlcAvailable())
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.SaveLoadTexts.DlcRequired, addToLog: false, WarningNotificationFormat.Attention);
			});
		}
		else
		{
			CurrentSave.Value = saveSlotVM;
			PhotonManager.Save.SelectSave(saveInfo);
		}
	}

	public void OnAreaBeginUnloading()
	{
		m_CloseAction?.Invoke();
	}

	public void OnAreaDidLoad()
	{
		m_CloseAction?.Invoke();
	}

	public void RequestRole()
	{
		EventBus.RaiseEvent(delegate(INetRolesRequest h)
		{
			h.HandleNetRolesRequest();
		});
	}

	public void HandlePlayerEnteredRoom(Photon.Realtime.Player player)
	{
		SetPlayers();
	}

	public void HandlePlayerLeftRoom(Photon.Realtime.Player player)
	{
		SetPlayers();
	}

	public void HandlePlayerChanged()
	{
		SetPlayers();
	}

	public void HandleLastPlayerLeftLobby()
	{
	}

	public void HandleRoomOwnerChanged()
	{
		UpdateRoom();
	}

	public void HandleSaveSelect(SaveInfo saveInfo)
	{
		if (!IsHost.Value)
		{
			CurrentSave.Value?.Dispose();
			CurrentSave.Value = ((saveInfo != null) ? new SaveSlotVM(saveInfo, new ReactiveProperty<SaveLoadMode>()) : null);
		}
	}

	public void ShowNetLobbyTutorial()
	{
		NetLobbyTutorialPartVM.Value = new NetLobbyTutorialPartVM(HideNetLobbyTutorial);
		NetLobbyTutorialOnScreen.Value = true;
		void HideNetLobbyTutorial()
		{
			SetFirstLaunchPrefs();
			NetLobbyTutorialPartVM.Value?.Dispose();
			NetLobbyTutorialPartVM.Value = null;
			NetLobbyTutorialOnScreen.Value = false;
		}
	}

	[Cheat(Name = "clear_net_lobby_tutorial")]
	public static void ClearFirstLaunchPrefs()
	{
		PlayerPrefs.SetInt("first_open_net_lobby_tutorial", 0);
		PlayerPrefs.Save();
	}

	[Cheat(Name = "set_net_lobby_tutorial")]
	public static void SetFirstLaunchPrefs()
	{
		PlayerPrefs.SetInt("first_open_net_lobby_tutorial", 1);
		PlayerPrefs.Save();
	}

	private void CheckEnoughPlayers()
	{
		IsEnoughPlayersForGame.Value = PhotonManager.Instance.IsEnoughPlayersForGame;
	}

	public async void OpenEpicGamesLayer()
	{
		Platform secondaryPlatform = PlatformServices.Platform.SecondaryPlatform;
		if (!secondaryPlatform.IsInitialized())
		{
			await PlatformServices.Platform.InitSecondary();
		}
		secondaryPlatform.Invite.ShowInviteWindow();
	}

	private void CheckProblemsWithMods()
	{
		Dictionary<string, ModData[]> dictionary = new Dictionary<string, ModData[]>();
		foreach (PlayerInfo allPlayer in PhotonManager.Instance.AllPlayers)
		{
			PhotonManager.Mods.TryGetModsData(allPlayer.UserId, out var mods);
			dictionary.TryAdd(allPlayer.UserId, mods);
		}
		List<ModData[]> list = (from pair in dictionary
			select new
			{
				UserId = pair.Key,
				Mods = pair.Value
			} into user
			group user by user.Mods).SelectMany(group => group.Select(user => user.Mods)).ToList();
		if (!list.Any())
		{
			PlayersDifferentMods.Value = string.Empty;
			return;
		}
		List<string> list2 = new List<string>();
		int num = 0;
		foreach (ModData[] item2 in list)
		{
			if (item2 == null || !item2.Any())
			{
				continue;
			}
			ModData[] array = item2;
			foreach (ModData mod in array)
			{
				if (!list2.Any((string s) => s.Contains(mod.ToString())))
				{
					string item = $"{num + 1}. {mod}";
					list2.Add(item);
					num++;
				}
			}
		}
		PlayersDifferentMods.Value = ((!list2.Any()) ? string.Empty : (UIStrings.Instance.NetLobbyTexts.CanBeAProblemsWithMods.Text + ":" + Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, list2)));
	}

	public void ShowDlcList()
	{
		PlayerInfo playerInfo = PhotonManager.Instance.AllPlayers.FirstOrDefault((PlayerInfo p) => p.Player.ActorNumber == PhotonManager.Instance.MasterClientId);
		if (!string.IsNullOrWhiteSpace(playerInfo.UserId))
		{
			ReactiveProperty<NetLobbyDlcListVM> dlcListVM = DlcListVM;
			if (dlcListVM.Value == null)
			{
				NetLobbyDlcListVM netLobbyDlcListVM2 = (dlcListVM.Value = new NetLobbyDlcListVM(CloseAction, GetPlayerDlcs(playerInfo.UserId)));
			}
		}
		void CloseAction()
		{
			DlcListVM.Value?.Dispose();
			DlcListVM.Value = null;
		}
	}

	private List<IBlueprintDlc> GetPlayerDlcs(string userID)
	{
		if (PhotonManager.DLC.TryGetPlayerDLC(userID, out var playerDLCs))
		{
			return playerDLCs;
		}
		PFLog.UI.Log("[NetLobbyPlayerVM.RefreshDLCsList] DLCs for user='" + userID + "' not found!");
		return new List<IBlueprintDlc>();
	}

	private void CompareHostAndPlayerDlcs()
	{
		m_DifferentDlcWithSaveProblems.Clear();
		ReadonlyList<PlayerInfo> allPlayers = PhotonManager.Instance.AllPlayers;
		PlayerInfo playerInfo = allPlayers.FirstOrDefault((PlayerInfo p) => p.Player.ActorNumber == PhotonManager.Instance.MasterClientId);
		if (string.IsNullOrWhiteSpace(playerInfo.UserId))
		{
			CheckProblemsWithDlcs.Execute(m_DifferentDlcWithSaveProblems);
			return;
		}
		List<BlueprintDlc> first = GetPlayerDlcs(playerInfo.UserId).OfType<BlueprintDlc>().Where(delegate(BlueprintDlc dlc)
		{
			DlcTypeEnum dlcType = dlc.DlcType;
			return dlcType != DlcTypeEnum.CosmeticDlc && dlcType != DlcTypeEnum.PromotionalDlc;
		}).ToList();
		foreach (PlayerInfo item in allPlayers)
		{
			if (playerInfo.UserId == item.UserId)
			{
				continue;
			}
			List<IBlueprintDlc> playerDlcs = GetPlayerDlcs(item.UserId);
			List<IBlueprintDlc> list = first.Except(playerDlcs).ToList();
			if (list.Any())
			{
				string nickName;
				string key = ((PhotonManager.Player.GetNickName(item.Player, out nickName) && !string.IsNullOrWhiteSpace(nickName)) ? nickName : item.UserId);
				if (!m_DifferentDlcWithSaveProblems.ContainsKey(key))
				{
					m_DifferentDlcWithSaveProblems.Add(key, list);
				}
				else
				{
					m_DifferentDlcWithSaveProblems[key].AddRange(list);
				}
			}
		}
		CheckProblemsWithDlcs.Execute(m_DifferentDlcWithSaveProblems);
	}

	public void HandleSetEpicGamesButtonActive(bool state)
	{
		EpicGamesButtonActive.Value = state;
	}

	public void HandleSetEpicGamesUserName(bool isAuthorized, string name)
	{
		EpicGamesAuthorized.Value = isAuthorized;
		EpicGamesUserName.Value = name;
	}

	public void HandleCheckUsersMods()
	{
		CheckProblemsWithMods();
		CompareHostAndPlayerDlcs();
	}

	public void HandleNetLobbyRequest(bool isMainMenu = false)
	{
	}

	public void HandleNetLobbyClose()
	{
		OnClose();
	}
}
