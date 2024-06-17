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
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.Networking.Platforms;
using Kingmaker.Networking.Player;
using Kingmaker.Networking.Save;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.UI.MVVM;
using Photon.Realtime;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.NetLobby;

public class NetLobbyVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, INetEvents, ISubscriber, ISavesUpdatedHandler, IAreaHandler, INetLobbyPlayersHandler, INetSaveSelectHandler, INetLobbyEpicGamesEvents, INetCheckUsersModsHandler, INetLobbyRequest
{
	private const string NET_LOBBY_TUTORIAL_PREF_KEY = "first_open_net_lobby_tutorial";

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

	private readonly List<SaveSlotVM> m_SaveSlotVMs = new List<SaveSlotVM>();

	private bool m_NeedUpdateRegion = true;

	public readonly ReactiveProperty<NetLobbyTutorialPartVM> NetLobbyTutorialPartVM = new ReactiveProperty<NetLobbyTutorialPartVM>();

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
		IsAnyTutorialBlocks.Value = Enumerable.Any(UIConfig.Instance.BlueprintUINetLobbyTutorial.TutorialBlocksInfo);
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
		AddDisposable(MainThreadDispatcher.UpdateAsObservable().Subscribe(delegate
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
	}

	protected override void DisposeImplementation()
	{
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
		DeactivateNetHandlers();
		PhotonManager.IdleConnection.LobbyViewOpened = false;
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
			if (itemVM is NetLobbyRegionDropdownVM netLobbyRegionDropdownVM)
			{
				PhotonManager.NetGame.ChangeRegion(netLobbyRegionDropdownVM.Region);
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
			DisposeSaveSloCollection();
		}
		else if (PhotonManager.NetGame.CurrentState == NetGame.State.InLobby)
		{
			UIUtility.ShowMessageBox(UIStrings.Instance.NetLobbyTexts.LeaveLobbyMessageBox, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
			{
				if (button == DialogMessageBoxBase.BoxButton.Yes)
				{
					Disconnect();
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
		PhotonManager.NetGame.CreateNewLobby();
	}

	public void JoinLobby()
	{
		NetRoomNameHelper.TryParse(m_Code, PhotonManager.Instance.RegionHandler.EnabledRegions, out var server, out var room);
		PhotonManager.Invite.AcceptInvite(server, room);
	}

	public void StopWaiting()
	{
		Disconnect();
	}

	public void Disconnect()
	{
		PhotonManager.Instance.StopPlaying();
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
		if ((PhotonManager.Sync.HasDesync || IsSaveAllowed.Value) && !IsMainMenu)
		{
			bool num2 = Game.Instance.Player.CheckDlcAvailable();
			bool isDlcsInLobbyReady = PhotonManager.DLC.IsDLCsInLobbyReady;
			if (!num2 || !isDlcsInLobbyReady)
			{
				EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
				{
					h.HandleWarning((!isDlcsInLobbyReady) ? UIStrings.Instance.SaveLoadTexts.DlcListIsNotLoading : UIStrings.Instance.SaveLoadTexts.DlcRequired, addToLog: false, WarningNotificationFormat.Attention);
				});
				return false;
			}
			bool flag2 = PhotonManager.NetGame.StartGameWithoutSave();
			CanConfirmLaunch.Value = flag2;
			return flag2;
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
		SaveSlotCollectionVm.Value = new SaveSlotCollectionVM(SaveLoadMode, CurrentSave, delegate
		{
			OnClose();
		});
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
		NetGameCurrentState.Value = state;
		IsPlayingState.Value = state == NetGame.State.Playing;
	}

	public void HandleNLoadingScreenClosed()
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
			List<SaveInfo> referenceCollection = new List<SaveInfo>(Game.Instance.SaveManager);
			referenceCollection.RemoveAll(SaveManager.IsCoopSave);
			referenceCollection.Sort((SaveInfo s1, SaveInfo s2) => -s1.SystemSaveTime.CompareTo(s2.SystemSaveTime));
			bool allowSwitchOff = Game.Instance.ControllerMode == Game.ControllerModeType.Gamepad;
			foreach (SaveInfo saveInfo in referenceCollection)
			{
				if (!Enumerable.Any(m_SaveSlotVMs, (SaveSlotVM vm) => vm.ReferenceSaveEquals(saveInfo)))
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
						}
					}, allowSwitchOff);
					AddDisposable(saveSlotVM);
					SaveSlotCollectionVm.Value.HandleNewSave(saveSlotVM);
					m_SaveSlotVMs.Add(saveSlotVM);
				}
			}
			List<SaveSlotVM> list = new List<SaveSlotVM>();
			foreach (SaveSlotVM item in m_SaveSlotVMs.Where((SaveSlotVM saveSlotVm) => !Enumerable.Any(referenceCollection, saveSlotVm.ReferenceSaveEquals)))
			{
				item.Dispose();
				list.Add(item);
			}
			foreach (SaveSlotVM item2 in list)
			{
				SaveSlotCollectionVm.Value.HandleDeleteSave(item2);
				m_SaveSlotVMs.Remove(item2);
			}
			SaveListAreEmpty.Value = m_SaveSlotVMs.Count == 0;
		}));
	}

	private void SetSelectSaveAction(SaveInfo saveInfo)
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
		NetLobbyTutorialPartVM.Value = new NetLobbyTutorialPartVM(delegate
		{
			HideNetLobbyTutorial();
		});
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
		if (PhotonManager.Mods.IsSameMods)
		{
			PlayersDifferentMods.Value = string.Empty;
			return;
		}
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
		if (!Enumerable.Any(list))
		{
			PlayersDifferentMods.Value = string.Empty;
			return;
		}
		List<string> list2 = new List<string>();
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			ModData[] array = list[i];
			for (int j = 0; j < array.Length; j++)
			{
				string item = $"{num + 1}. {array[j]}";
				list2.Add(item);
				num++;
			}
		}
		if (Enumerable.Any(list2))
		{
			PlayersDifferentMods.Value = UIStrings.Instance.NetLobbyTexts.CanBeAProblemsWithMods.Text + ":" + Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, list2);
		}
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
	}

	public void HandleNetLobbyRequest(bool isMainMenu = false)
	{
	}

	public void HandleNetLobbyClose()
	{
		OnClose();
	}
}
