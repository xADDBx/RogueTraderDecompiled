using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Cheats;
using ExitGames.Client.Photon;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.GameInfo;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.Networking.Platforms;
using Kingmaker.Networking.Platforms.User;
using Kingmaker.Networking.Player;
using Kingmaker.Networking.Settings;
using Kingmaker.Networking.Tools;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Stores;
using Kingmaker.UI.Common;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Reporting.Base;
using Photon.Realtime;
using UnityEngine;

namespace Kingmaker.Networking;

public class PhotonManager : MonoBehaviour, IMatchmakingCallbacks, IConnectionCallbacks, IInRoomCallbacks, IOnEventCallback
{
	public static class RoomProperties
	{
		public const string Settings = "st";

		public const string SaveInfo = "si";

		public const string PlatformSession = "ps";
	}

	public static class UserProperties
	{
		public const string Store = "s";

		public const string DLC = "d";

		public const string Mods = "m";

		public const string TimeOffset = "to";

		public const string PlatformPicture = "pp";
	}

	private struct ProcessDeltaTimeLogic
	{
		private readonly PhotonManager m_PhotonManager;

		private long m_TimeOffsetLastValue;

		public ProcessDeltaTimeLogic(PhotonManager photonManager)
		{
			m_PhotonManager = photonManager;
			m_TimeOffsetLastValue = 0L;
		}

		public TimeSpan ProcessDeltaTime(TimeSpan lastTickTime, TimeSpan deltaTime)
		{
			return deltaTime;
		}

		private bool GetTimeOffset(out long timeOffset)
		{
			timeOffset = long.MinValue;
			foreach (PlayerInfo activePlayer in m_PhotonManager.ActivePlayers)
			{
				if (m_PhotonManager.ActorNumberToPhotonPlayer(activePlayer.Player, out var player) && m_PhotonManager.GetPlayerProperty<long>(player, "to", out var obj))
				{
					timeOffset = Math.Max(timeOffset, obj);
				}
			}
			return timeOffset != long.MinValue;
		}

		public void Reset()
		{
			m_TimeOffsetLastValue = 0L;
			m_PhotonManager.ClearPlayerProperty("to");
		}
	}

	private class PlayerLeavingController
	{
		private readonly struct Data
		{
			public readonly PhotonActorNumber ActorNumber;

			public readonly TaskCompletionSource<bool> TaskCompletionSource;

			public Data(PhotonActorNumber actorNumber)
			{
				ActorNumber = actorNumber;
				TaskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			}
		}

		private readonly PhotonManager m_PhotonManager;

		private readonly List<Data> m_Data;

		public PlayerLeavingController(PhotonManager photonManager)
		{
			m_PhotonManager = photonManager;
			m_Data = new List<Data>();
		}

		public Task WaitPlayerLeft(PhotonActorNumber actorNumber)
		{
			bool flag = false;
			foreach (PlayerInfo activePlayer in m_PhotonManager.ActivePlayers)
			{
				if (activePlayer.Player == actorNumber)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return Task.CompletedTask;
			}
			Data item = new Data(actorNumber);
			m_Data.Add(item);
			return item.TaskCompletionSource.Task;
		}

		public void OnPlayerLeftRoom(PhotonActorNumber photonActorNumber)
		{
			for (int i = 0; i < m_Data.Count; i++)
			{
				if (m_Data[i].ActorNumber == photonActorNumber)
				{
					m_Data[i].TaskCompletionSource.TrySetResult(result: true);
					m_Data.RemoveAt(i);
					i--;
				}
			}
		}
	}

	public static readonly MessageNetManager Message = new MessageNetManager();

	public static readonly CommandNetManager Command = new CommandNetManager();

	public static readonly SaveNetManager Save = new SaveNetManager();

	public static readonly SyncNetManager Sync = new SyncNetManager();

	public static readonly LobbyNetManager Lobby = new LobbyNetManager();

	public static readonly LockNetManager Lock = new LockNetManager();

	public static readonly SettingsNetManager Settings = new SettingsNetManager();

	public static readonly PlayerNetManager Player = new PlayerNetManager();

	public static readonly PingNetManager Ping = new PingNetManager();

	public static readonly InviteNetManager Invite = new InviteNetManager();

	public static readonly CheatState Cheat = new CheatState();

	public static readonly DlcNetManager DLC = new DlcNetManager();

	public static readonly ModsNetManager Mods = new ModsNetManager();

	public static readonly BugReportNetManager BugReport = new BugReportNetManager();

	public static readonly NetGame NetGame = new NetGame();

	public static readonly IdleConnectionChecker IdleConnection = new IdleConnectionChecker(NetGame);

	public static PhotonAppIdRealtime AppIdRealtime = default(PhotonAppIdRealtime);

	private static PhotonManager s_Instance;

	private static readonly RaiseEventOptions OneReceiver = new RaiseEventOptions
	{
		TargetActors = new int[1]
	};

	private static readonly RaiseEventOptions AllReceivers = new RaiseEventOptions
	{
		Receivers = ReceiverGroup.All
	};

	private static readonly RaiseEventOptions OtherReceivers = new RaiseEventOptions
	{
		TargetActors = Array.Empty<int>()
	};

	public static Action<string> OnStartCoopSession = delegate
	{
	};

	private readonly LoadBalancingClient m_LoadBalancingClient = new LoadBalancingClient();

	private readonly List<PlayerInfo> m_ActivePlayers = new List<PlayerInfo>(8);

	private readonly List<PlayerInfo> m_AllPlayersCache = new List<PlayerInfo>(8);

	private readonly List<PhotonActorNumber> m_PhotonActorNumbersAtStart = new List<PhotonActorNumber>(8);

	private BackgroundPing m_BackgroundPing;

	private ConnectionCallbacksAsync m_ConnectionCallbacksAsync;

	private MatchmakingCallbacksAsync m_MatchmakingCallbacksAsync;

	private PhotonStatsLogger m_StatsLogger;

	private ProcessDeltaTimeLogic m_ProcessDeltaTimeLogic;

	private PlayerLeavingController m_PlayerLeavingController;

	public static readonly string Version = GetVersion();

	public static int MaxPlayers { get; private set; } = 6;


	public static PhotonManager Instance => s_Instance;

	public static bool Initialized
	{
		get
		{
			if (s_Instance != null)
			{
				return s_Instance.m_LoadBalancingClient.IsConnectedAndReady;
			}
			return false;
		}
	}

	public static bool ReadyToHostOrJoin
	{
		get
		{
			if (Initialized)
			{
				return s_Instance.m_LoadBalancingClient.State == ClientState.ConnectedToMasterServer;
			}
			return false;
		}
	}

	public IPlatformUser LocalPlatformUser => PlatformServices.Platform.User;

	public ByteArraySlicePool ByteArraySlicePool => m_LoadBalancingClient.LoadBalancingPeer.ByteArraySlicePool;

	public bool InRoom => m_LoadBalancingClient.InRoom;

	public string RoomName => m_LoadBalancingClient.CurrentRoom?.Name;

	public bool IsConnected => m_LoadBalancingClient.IsConnected;

	public bool IsInStableState
	{
		get
		{
			if (!m_LoadBalancingClient.IsConnectedAndReady)
			{
				return m_LoadBalancingClient.State == ClientState.Disconnected;
			}
			return true;
		}
	}

	public bool IsRoomOpen
	{
		get
		{
			return m_LoadBalancingClient.CurrentRoom?.IsOpen ?? false;
		}
		set
		{
			if (m_LoadBalancingClient.CurrentRoom != null)
			{
				m_LoadBalancingClient.CurrentRoom.IsOpen = value;
			}
		}
	}

	public int MasterClientId => m_LoadBalancingClient.CurrentRoom?.MasterClientId ?? 0;

	public int LocalClientId => m_LoadBalancingClient.LocalPlayer.ActorNumber;

	public NetPlayer LocalNetPlayer
	{
		get
		{
			if (m_LoadBalancingClient.InRoom)
			{
				int num = ActorNumberToPlayerIndex(m_LoadBalancingClient.LocalPlayer.ActorNumber);
				if (0 < num)
				{
					return new NetPlayer(num, isLocal: true);
				}
			}
			return NetPlayer.Offline;
		}
	}

	public string LocalPlayerUserId => m_LoadBalancingClient.UserId;

	public int PlayerCount => m_LoadBalancingClient.CurrentRoom?.PlayerCount ?? 1;

	public NetPlayer RoomOwner
	{
		get
		{
			if (!m_LoadBalancingClient.InRoom)
			{
				return NetPlayer.Offline;
			}
			return new PhotonActorNumber(m_LoadBalancingClient.CurrentRoom.MasterClientId).ToNetPlayer(NetPlayer.Empty);
		}
	}

	public bool IsRoomOwner
	{
		get
		{
			if (m_LoadBalancingClient.InRoom)
			{
				return m_LoadBalancingClient.CurrentRoom.MasterClientId == m_LoadBalancingClient.LocalPlayer.ActorNumber;
			}
			return true;
		}
	}

	public string Region => m_LoadBalancingClient.CloudRegion;

	public RegionHandler RegionHandler
	{
		get
		{
			RegionHandler regionHandler = m_LoadBalancingClient.RegionHandler;
			if (regionHandler == null || regionHandler.IsPinging)
			{
				return null;
			}
			return m_LoadBalancingClient.RegionHandler;
		}
	}

	public NetPlayerGroup PlayersReadyMask
	{
		get
		{
			NetPlayerGroup result = NetPlayerGroup.Offline;
			int i = 0;
			for (int count = m_ActivePlayers.Count; i < count; i++)
			{
				PhotonActorNumber player = m_ActivePlayers[i].Player;
				result = result.Add(player.ToNetPlayer(NetPlayer.Empty));
			}
			return result;
		}
	}

	public PhotonTrafficStats PhotonTrafficStats { get; private set; }

	public PhotonNetworkStats PhotonNetworkStats { get; private set; }

	public DataTransporter DataTransporter { get; private set; }

	public CharGenPortraitSyncer PortraitSyncer { get; private set; }

	public ReadonlyList<PlayerInfo> ActivePlayers => m_ActivePlayers;

	public ReadonlyList<PhotonActorNumber> PhotonActorNumbersAtStart => m_PhotonActorNumbersAtStart;

	public ReadonlyList<PlayerInfo> AllPlayers
	{
		get
		{
			List<PlayerInfo> allPlayersCache = m_AllPlayersCache;
			allPlayersCache.Clear();
			Room currentRoom = m_LoadBalancingClient.CurrentRoom;
			if (currentRoom == null)
			{
				return allPlayersCache;
			}
			Dictionary<int, Photon.Realtime.Player> players = currentRoom.Players;
			allPlayersCache.IncreaseCapacity(players.Count);
			foreach (KeyValuePair<int, Photon.Realtime.Player> item in players)
			{
				PhotonActorNumber photonActorNumber = new PhotonActorNumber(item.Value.ActorNumber);
				bool isActive = PhotonActorNumbersAtStart.Contains(photonActorNumber);
				allPlayersCache.Add(new PlayerInfo(photonActorNumber, item.Value.UserId, item.Value.NickName, isActive));
			}
			allPlayersCache.Sort();
			return allPlayersCache;
		}
	}

	public bool IsEnoughPlayersForGame
	{
		get
		{
			if (!Cheat.AllowRunWithOnePlayer)
			{
				return m_LoadBalancingClient.CurrentRoom.PlayerCount > 1;
			}
			return true;
		}
	}

	public NetworkSimulationSet SimulationSettings => m_LoadBalancingClient.LoadBalancingPeer.NetworkSimulationSettings;

	public bool IsSimulationEnabled
	{
		get
		{
			return m_LoadBalancingClient.LoadBalancingPeer.IsSimulationEnabled;
		}
		set
		{
			m_LoadBalancingClient.LoadBalancingPeer.IsSimulationEnabled = value;
		}
	}

	[Cheat(Name = "net_max_players", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void MaxPlayersCheat(int value = 6)
	{
		MaxPlayers = Mathf.Clamp(value, 1, 6);
	}

	public PhotonManager()
	{
		m_ProcessDeltaTimeLogic = new ProcessDeltaTimeLogic(this);
	}

	public static void CreateInstance()
	{
		if (BuildModeUtility.IsCoopEnabled)
		{
			if (s_Instance != null)
			{
				PFLog.Net.Error("Another instance of PhotonManager already initialized!");
				return;
			}
			PFLog.Net.Log("Creating PhotonManager Instance...");
			s_Instance = new GameObject("PhotonManager").AddComponent<PhotonManager>();
			UnityEngine.Object.DontDestroyOnLoad(s_Instance);
			s_Instance.Init();
		}
	}

	private void Init()
	{
		m_LoadBalancingClient.AddCallbackTarget(this);
		m_LoadBalancingClient.LoadBalancingPeer.ReuseEventInstance = true;
		m_LoadBalancingClient.LoadBalancingPeer.UseByteArraySlicePoolForEvents = true;
		m_LoadBalancingClient.StateChanged += delegate(ClientState a, ClientState b)
		{
			PFLog.Net.Log($"OnStateChanged({a}, {b})");
		};
		m_BackgroundPing = new BackgroundPing(m_LoadBalancingClient);
		PhotonTrafficStats = new PhotonTrafficStats(m_LoadBalancingClient);
		PhotonNetworkStats = new PhotonNetworkStats(m_LoadBalancingClient);
		m_StatsLogger = new PhotonStatsLogger(m_LoadBalancingClient);
		m_ConnectionCallbacksAsync = new ConnectionCallbacksAsync(m_LoadBalancingClient);
		m_MatchmakingCallbacksAsync = new MatchmakingCallbacksAsync(m_LoadBalancingClient);
		DataTransporter = new DataTransporter(this, CustomPortraitsManager.Instance);
		PortraitSyncer = new CharGenPortraitSyncer(this, CustomPortraitsManager.Instance);
		m_PlayerLeavingController = new PlayerLeavingController(this);
	}

	public Task ConnectAsync(AuthenticationValues authenticationValues)
	{
		m_LoadBalancingClient.AuthValues = authenticationValues;
		string id = AppIdRealtime.Id;
		string version = Version;
		bool num = m_LoadBalancingClient.ConnectUsingSettings(new AppSettings
		{
			AppIdRealtime = id,
			AppVersion = version
		});
		PFLog.Net.Log(string.Format("Photon AppId='{0}' AppVersion={1} IsReleaseAppId='{2}'", (id.Length > 8) ? (id.Substring(0, 8) + "...") : id, version, AppIdRealtime.IsRelease));
		if (!num)
		{
			throw new SendMessageFailException("Can't send Connect");
		}
		return m_ConnectionCallbacksAsync.WaitConnect();
	}

	public void Update()
	{
		if (m_LoadBalancingClient.State != 0)
		{
			m_StatsLogger.Update();
			Receive();
			Send();
			m_BackgroundPing.ResetTime();
			if (IdleConnection.ShouldDisconnect())
			{
				PFLog.Net.Log("[PhotonManager.Update] Idle state, disconnecting");
				m_LoadBalancingClient.Disconnect();
			}
		}
	}

	public void Receive()
	{
		while (m_LoadBalancingClient.LoadBalancingPeer.DispatchIncomingCommands())
		{
		}
	}

	public void Send()
	{
		while (m_LoadBalancingClient.LoadBalancingPeer.SendOutgoingCommands())
		{
		}
	}

	private void OnDestroy()
	{
		if (!(s_Instance != this))
		{
			m_ConnectionCallbacksAsync.Dispose();
			m_MatchmakingCallbacksAsync.Dispose();
			m_BackgroundPing.Dispose();
			m_LoadBalancingClient.Disconnect();
			m_LoadBalancingClient.RemoveCallbackTarget(this);
		}
	}

	void IMatchmakingCallbacks.OnFriendListUpdate(List<FriendInfo> friendList)
	{
		PFLog.Net.Log("PhotonManager.OnFriendListUpdate()");
	}

	void IMatchmakingCallbacks.OnCreatedRoom()
	{
		PFLog.Net.Log("PhotonManager.OnCreatedRoom()");
	}

	void IMatchmakingCallbacks.OnCreateRoomFailed(short returnCode, string message)
	{
		PFLog.Net.Log($"PhotonManager.OnCreateRoomFailed({returnCode}, {message})");
	}

	void IMatchmakingCallbacks.OnJoinedRoom()
	{
		PFLog.Net.Log("PhotonManager.OnJoinedRoom()");
	}

	void IMatchmakingCallbacks.OnJoinRoomFailed(short returnCode, string message)
	{
		PFLog.Net.Log($"PhotonManager.OnJoinRoomFailed({returnCode}, {message})");
	}

	void IMatchmakingCallbacks.OnJoinRandomFailed(short returnCode, string message)
	{
		PFLog.Net.Log($"PhotonManager.OnJoinRandomFailed({returnCode}, {message})");
	}

	void IMatchmakingCallbacks.OnLeftRoom()
	{
	}

	public void OnJoinedLobby()
	{
		PhotonTrafficStats.Reset();
		PlatformServices.Platform.Session.OnJoinedLobby();
		m_LoadBalancingClient.NickName = PlatformServices.Platform.User.NickName;
		m_LoadBalancingClient.LocalPlayer.SetProperty("s", StoreManager.Store);
		DataTransporter.OnJoinedLobby(m_LoadBalancingClient.CurrentRoom.Players);
		DLC.OnJoinedLobby();
		Mods.OnJoinedLobby();
		Save.OnSelectedSaveUpdated();
	}

	void IConnectionCallbacks.OnConnected()
	{
		PlatformServices.Platform.AuthService.OnConnected();
		PlayerAvatar largeIcon = PlatformServices.Platform.User.LargeIcon;
		Player.SetIconLarge(LocalPlayerUserId, largeIcon);
	}

	void IConnectionCallbacks.OnConnectedToMaster()
	{
	}

	void IConnectionCallbacks.OnDisconnected(DisconnectCause cause)
	{
		PFLog.Net.Log("PhotonManager.OnDisconnected() cause=" + cause);
		if (cause != DisconnectCause.DisconnectByClientLogic && cause != 0)
		{
			NetGame.State currentState = NetGame.CurrentState;
			bool allowReconnect = currentState != NetGame.State.Playing && currentState != NetGame.State.UploadSaveAndStartLoading && currentState != NetGame.State.DownloadSaveAndLoading;
			UINetUtility.HandlePhotonDisconnectedError(cause, allowReconnect);
		}
		StopPlaying("OnDisconnected");
	}

	void IConnectionCallbacks.OnRegionListReceived(RegionHandler regionHandler)
	{
	}

	void IConnectionCallbacks.OnCustomAuthenticationResponse(Dictionary<string, object> data)
	{
	}

	void IConnectionCallbacks.OnCustomAuthenticationFailed(string debugMessage)
	{
	}

	void IInRoomCallbacks.OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
	{
		EventBus.RaiseEvent(delegate(INetLobbyPlayersHandler h)
		{
			h.HandlePlayerEnteredRoom(newPlayer);
		});
		Save.OnPlayerEnteredRoom(new PhotonActorNumber(newPlayer.ActorNumber));
	}

	void IInRoomCallbacks.OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
	{
		PhotonActorNumber photonActorNumber = new PhotonActorNumber(otherPlayer.ActorNumber);
		NetPlayer netPlayer = photonActorNumber.ToNetPlayer(NetPlayer.Empty);
		PFLog.Net.Log($"[OnPlayerLeftRoom] '{otherPlayer.UserId}' {otherPlayer.ActorNumber}, {netPlayer}");
		int i = 0;
		for (int count = m_ActivePlayers.Count; i < count; i++)
		{
			if (m_ActivePlayers[i].Player == photonActorNumber)
			{
				m_ActivePlayers.RemoveAt(i);
				break;
			}
		}
		m_PlayerLeavingController.OnPlayerLeftRoom(photonActorNumber);
		Save.OnPlayerLeftRoom(photonActorNumber);
		DLC.OnPlayerLeftRoom(otherPlayer);
		Mods.OnPlayerLeftRoom(otherPlayer);
		PortraitSyncer.OnPlayerLeftRoom(photonActorNumber);
		DataTransporter.OnPlayerLeftRoom(photonActorNumber);
		StopPlayingIfLastPlayer();
		CheckRoomOwnerChanged(otherPlayer);
		EventBus.RaiseEvent(delegate(INetLobbyPlayersHandler h)
		{
			h.HandlePlayerLeftRoom(otherPlayer);
		});
		if (Lobby.IsPlaying)
		{
			Game.Instance.PauseController.OnPlayerLeftRoom();
			Game.Instance.CoopData.PlayerRole.OnPlayerLeftRoom(netPlayer);
		}
	}

	public bool StopPlayingIfLastPlayer()
	{
		bool firstLoadCompleted = Lobby.FirstLoadCompleted;
		bool flag = m_LoadBalancingClient.CurrentRoom.PlayerCount == 1;
		if (firstLoadCompleted && flag)
		{
			StopPlaying("StopPlayingIfLastPlayer");
			EventBus.RaiseEvent(delegate(INetLobbyPlayersHandler h)
			{
				h.HandleLastPlayerLeftLobby();
			});
			return true;
		}
		return false;
	}

	private void CheckRoomOwnerChanged(Photon.Realtime.Player otherPlayer)
	{
		if (MasterClientId > otherPlayer.ActorNumber)
		{
			EventBus.RaiseEvent(delegate(INetLobbyPlayersHandler h)
			{
				h.HandleRoomOwnerChanged();
			});
		}
	}

	void IInRoomCallbacks.OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
	{
		using DictionaryEntryEnumerator dictionaryEntryEnumerator = propertiesThatChanged.GetEnumerator();
		while (dictionaryEntryEnumerator.MoveNext() && dictionaryEntryEnumerator.Current.Key is string text)
		{
			switch (text)
			{
			case "st":
				Settings.OnRoomSettingsUpdate();
				break;
			case "si":
				Save.OnSelectedSaveUpdated();
				break;
			default:
				PFLog.Net.Error("[OnRoomPropertiesUpdate] Unexpected property '" + text + "'");
				break;
			case "ps":
				break;
			}
		}
	}

	void IInRoomCallbacks.OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
	{
		bool flag = false;
		foreach (DictionaryEntry changedProp in changedProps)
		{
			object key = changedProp.Key;
			if (key is byte)
			{
				if ((byte)key == byte.MaxValue)
				{
					flag = true;
					continue;
				}
			}
			else
			{
				switch (key as string)
				{
				case "s":
				{
					StoreType storeType = (StoreType)changedProp.Value;
					DataTransporter.OnStoreUpdated(targetPlayer, storeType);
					continue;
				}
				case "d":
					DLC.OnPlayerUpdate(targetPlayer);
					continue;
				case "m":
					Mods.OnPlayerUpdate(targetPlayer);
					continue;
				case "to":
				case "pp":
					continue;
				}
			}
			PFLog.Net.Error($"[OnRoomPropertiesUpdate] Unexpected property '{changedProp.Key}'");
		}
		if (flag)
		{
			EventBus.RaiseEvent(delegate(INetLobbyPlayersHandler h)
			{
				h.HandlePlayerChanged();
			});
		}
	}

	void IInRoomCallbacks.OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
	{
		PFLog.Net.Log("[OnMasterClientSwitched]");
		if (IsRoomOwner)
		{
			Invite.StartAnnounceGame();
		}
		else
		{
			Invite.StopAnnounceGame();
		}
	}

	void IOnEventCallback.OnEvent(EventData photonEvent)
	{
		byte code = photonEvent.Code;
		if (code < 200)
		{
			if (photonEvent.CustomData != null)
			{
				ByteArraySlice byteArraySlice = (ByteArraySlice)photonEvent.CustomData;
				ReadOnlySpan<byte> bytes = new ReadOnlySpan<byte>(byteArraySlice.Buffer, byteArraySlice.Offset, byteArraySlice.Count);
				Message.OnMessage(code, photonEvent.Sender, bytes);
				byteArraySlice.Release();
			}
			else
			{
				Message.OnMessage(code, photonEvent.Sender, ReadOnlySpan<byte>.Empty);
			}
		}
	}

	public bool SendMessageTo(PhotonActorNumber receiver, byte code)
	{
		return SendMessageTo(receiver, code, null, 0, 0);
	}

	public bool SendMessageTo(PhotonActorNumber receiver, byte code, byte[] bytes, int offset, int length)
	{
		RaiseEventOptions oneReceiver = OneReceiver;
		oneReceiver.TargetActors[0] = receiver.ActorNumber;
		return SendMessage(code, bytes, offset, length, oneReceiver);
	}

	public bool SendMessageToOthers(byte code)
	{
		return SendMessageToOthers(code, null, 0, 0);
	}

	public bool SendMessageToOthers(byte code, ByteArraySlice bytes)
	{
		return SendMessageToOthers(code, bytes.Buffer, bytes.Offset, bytes.Count);
	}

	public bool SendMessageToOthers(byte code, byte[] bytes, int offset, int length)
	{
		if (OtherReceivers.TargetActors.Length == 0)
		{
			return true;
		}
		return SendMessage(code, bytes, offset, length, OtherReceivers);
	}

	public bool SendMessageToEveryone(byte code)
	{
		return SendMessageToEveryone(code, null, 0, 0);
	}

	public bool SendMessageToEveryone(byte code, byte[] bytes, int offset, int length)
	{
		return SendMessage(code, bytes, offset, length, AllReceivers);
	}

	public bool SendMessage(byte code, byte[] bytes, int offset, int length, RaiseEventOptions sendOptions)
	{
		ByteArraySlice byteArraySlice = ((bytes != null) ? ByteArraySlicePool.Acquire(bytes, offset, length) : null);
		bool result = m_LoadBalancingClient.OpRaiseEvent(code, byteArraySlice, sendOptions, SendOptions.SendReliable);
		byteArraySlice?.Release();
		return result;
	}

	public void Reconnect()
	{
		m_LoadBalancingClient.ReconnectToMaster();
	}

	public bool JoinRandomRoom()
	{
		PFLog.Net.Log("Joining random room...");
		return m_LoadBalancingClient.OpJoinRandomRoom();
	}

	public bool JoinRoom(string roomName)
	{
		EnterRoomParams enterRoomParams = new EnterRoomParams
		{
			RoomName = roomName
		};
		return m_LoadBalancingClient.OpJoinRoom(enterRoomParams);
	}

	public void PrintPlayers()
	{
		Room currentRoom = m_LoadBalancingClient.CurrentRoom;
		if (currentRoom == null)
		{
			PFLog.Net.Error("The client isn't connected to any room!");
			return;
		}
		Dictionary<int, Photon.Realtime.Player> players = currentRoom.Players;
		PFLog.Net.Log($"Players (lobby) x{players.Count}:");
		int num = 1;
		foreach (KeyValuePair<int, Photon.Realtime.Player> item in players)
		{
			PFLog.Net.Log($" {num}) {item.Value.ActorNumber} #{item.Value.UserId} '{item.Value.NickName}'");
			num++;
		}
	}

	public bool PlayerToPhotonPlayer(NetPlayer player, out Photon.Realtime.Player photonPlayer)
	{
		return ActorNumberToPhotonPlayer(PlayerToActorNumber(player), out photonPlayer);
	}

	public bool ActorNumberToPhotonPlayer(PhotonActorNumber actorNumber, out Photon.Realtime.Player player)
	{
		if (!actorNumber.IsValid)
		{
			PFLog.Net.Warning("[ActorNumberToPhotonPlayer] PhotonActorNumber is invalid");
			player = null;
			return false;
		}
		Room currentRoom = m_LoadBalancingClient.CurrentRoom;
		if (currentRoom == null)
		{
			PFLog.Net.Warning("[ActorNumberToPhotonPlayer] CurrentRoom is null");
			player = null;
			return false;
		}
		bool num = currentRoom.Players.TryGetValue(actorNumber.ActorNumber, out player);
		if (!num)
		{
			PFLog.Net.Warning("[ActorNumberToPhotonPlayer] netPlayer=" + actorNumber.ToString() + " not found");
		}
		return num;
	}

	public void StopPlaying(string reason)
	{
		NetGame.StopPlaying(shouldLeaveLobby: true, reason);
	}

	public void Kick()
	{
		StopPlaying("Kick");
		UIUtility.ShowMessageBox(UIStrings.Instance.NetLobbyTexts.KickMessage, DialogMessageBoxBase.BoxType.Message, delegate
		{
		});
	}

	public void ClearState(bool shouldLeaveLobby)
	{
		PFLog.Net.Log("[PhotonManager.ClearState]");
		Sync.OnLeave();
		Lobby.OnLeave();
		Lock.OnLeave();
		DLC.OnLeave();
		Mods.OnLeave();
		if (shouldLeaveLobby)
		{
			DataTransporter.OnLeave();
			Invite.StopAnnounceGame();
			PlatformServices.Platform.Session.LeaveSession();
		}
		OtherReceivers.TargetActors = Array.Empty<int>();
		m_ActivePlayers.Clear();
		m_PhotonActorNumbersAtStart.Clear();
	}

	public Task LeaveRoomAsync()
	{
		if (m_LoadBalancingClient.InRoom)
		{
			PFLog.Net.Log("[PhotonManager.LeaveRoomAsync] send message");
			if (!m_LoadBalancingClient.OpLeaveRoom(becomeInactive: false))
			{
				throw new SendMessageFailException("can't send OpLeaveRoom");
			}
			return m_MatchmakingCallbacksAsync.WaitLeaveRoom();
		}
		PFLog.Net.Log("[PhotonManager.LeaveRoomAsync] leave not needed");
		return Task.CompletedTask;
	}

	private static string GetVersion()
	{
		if (!Application.isEditor)
		{
			return GameVersion.GetVersion() + "-" + GetStaticSettings();
		}
		return ReportVersionManager.GetCommitOrRevision(shortHash: true) + "-" + GetStaticSettings();
	}

	private static string GetStaticSettings()
	{
		return $"{ToInt(BuildModeUtility.IsDevelopment)}.{ToInt(BuildModeUtility.IncludeAllDesyncs)}.{GetPlatformCode()}";
		static int GetPlatformCode()
		{
			return 1;
		}
		static int ToInt(bool value)
		{
			if (!value)
			{
				return 0;
			}
			return 1;
		}
	}

	public void PrintRegions()
	{
		RegionHandler regionHandler2 = m_LoadBalancingClient.RegionHandler;
		if (regionHandler2 == null)
		{
			PFLog.Net.Error("RegionHandler is NULL");
		}
		else
		{
			regionHandler2.PingMinimumOfRegions(OnPingMinimumOfRegions, null);
		}
		static void OnPingMinimumOfRegions(RegionHandler regionHandler)
		{
			List<Region> enabledRegions = regionHandler.EnabledRegions;
			string text = $"PhotonManager.OnRegionListReceived() Regions x{enabledRegions.Count}:";
			int i = 0;
			for (int count = enabledRegions.Count; i < count; i++)
			{
				Region region = enabledRegions[i];
				text += $"\n {i}) '{region.Code}' p{region.Ping}";
			}
			PFLog.Net.Log(text);
		}
	}

	public async Task SetRegionAsync([NotNull] string region, [NotNull] AuthenticationValues authenticationValues)
	{
		if (!region.Equals(Region, StringComparison.Ordinal))
		{
			PFLog.Net.Log("Connect to region '" + region + "'.");
			m_LoadBalancingClient.AuthValues = authenticationValues;
			if (!m_LoadBalancingClient.ConnectToRegionMaster(region))
			{
				throw new SendMessageFailException("Failed to connect to '" + region + "' region server");
			}
			await m_ConnectionCallbacksAsync.WaitReconnect();
		}
	}

	public void KickPlayer(PhotonActorNumber kickPlayer)
	{
		SendMessageTo(kickPlayer, 9);
	}

	public void ContinueLoading()
	{
		if (!SendMessageToOthers(11))
		{
			PFLog.Net.Error("Error when trying to send сontinue loading!");
		}
	}

	public void OnContinueLoadingReceived()
	{
		EventBus.RaiseEvent(delegate(IContinueLoadingHandler h)
		{
			h.HandleContinueLoading();
		});
	}

	public void ClearRoomProperty(string propertyName)
	{
		if (m_LoadBalancingClient.InRoom)
		{
			ExitGames.Client.Photon.Hashtable propertiesToSet = new ExitGames.Client.Photon.Hashtable { [propertyName] = null };
			m_LoadBalancingClient.CurrentRoom.SetCustomProperties(propertiesToSet);
		}
	}

	public void SetRoomProperty<T>(string propertyName, T data)
	{
		if (m_LoadBalancingClient.InRoom)
		{
			ByteArraySlice byteArraySlice = NetMessageSerializer.SerializeToSlice(data);
			ExitGames.Client.Photon.Hashtable propertiesToSet = new ExitGames.Client.Photon.Hashtable { [propertyName] = byteArraySlice };
			m_LoadBalancingClient.CurrentRoom.SetCustomProperties(propertiesToSet);
			byteArraySlice.Release();
		}
	}

	public void SetPlayerProperty<T>(string propertyName, T data)
	{
		if (m_LoadBalancingClient.InRoom)
		{
			ByteArraySlice byteArraySlice = NetMessageSerializer.SerializeToSlice(data);
			ExitGames.Client.Photon.Hashtable propertiesToSet = new ExitGames.Client.Photon.Hashtable { [propertyName] = byteArraySlice };
			m_LoadBalancingClient.LocalPlayer.SetCustomProperties(propertiesToSet);
			byteArraySlice.Release();
		}
	}

	public void ClearPlayerProperty(string propertyName)
	{
		if (m_LoadBalancingClient.InRoom)
		{
			ExitGames.Client.Photon.Hashtable propertiesToSet = new ExitGames.Client.Photon.Hashtable { [propertyName] = null };
			m_LoadBalancingClient.LocalPlayer.SetCustomProperties(propertiesToSet);
		}
	}

	public T GetRoomPropertyOrDefault<T>(string propertyName, T defaultValue)
	{
		if (!GetRoomProperty<T>(propertyName, out var obj))
		{
			return defaultValue;
		}
		return obj;
	}

	public bool GetRoomProperty<T>(string propertyName, out T obj)
	{
		obj = default(T);
		if (m_LoadBalancingClient.InRoom)
		{
			return GetProperty<T>(m_LoadBalancingClient.CurrentRoom.CustomProperties, propertyName, out obj);
		}
		return false;
	}

	public bool GetPlayerProperty<T>(string propertyName, out T obj)
	{
		return GetPlayerProperty<T>(m_LoadBalancingClient.LocalPlayer, propertyName, out obj);
	}

	public bool GetPlayerProperty<T>(Photon.Realtime.Player player, string propertyName, out T obj)
	{
		obj = default(T);
		if (m_LoadBalancingClient.InRoom)
		{
			return GetProperty<T>(player.CustomProperties, propertyName, out obj);
		}
		return false;
	}

	private static bool GetProperty<T>(ExitGames.Client.Photon.Hashtable hashtable, string propertyName, out T obj)
	{
		if (hashtable.TryGetValue(propertyName, out var value))
		{
			byte[] array = (byte[])value;
			ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(array, 0, array.Length);
			obj = NetMessageSerializer.DeserializeFromSpan<T>(span);
			return true;
		}
		obj = default(T);
		return false;
	}

	public PhotonActorNumber[] GetActorNumbersAtStart()
	{
		Room currentRoom = m_LoadBalancingClient.CurrentRoom;
		if (currentRoom == null)
		{
			return null;
		}
		Dictionary<int, Photon.Realtime.Player> players = currentRoom.Players;
		PhotonActorNumber[] array = new PhotonActorNumber[players.Count + 1];
		array[0] = PhotonActorNumber.Invalid;
		int num = 1;
		foreach (KeyValuePair<int, Photon.Realtime.Player> item in players)
		{
			int key = item.Key;
			array[num++] = new PhotonActorNumber(key);
		}
		Array.Sort(array);
		return array;
	}

	public void CloseRoom(PhotonActorNumber[] actorNumbersAtStart, string[] dlcs)
	{
		PFLog.Net.Log("[PhotonManager.CloseRoom] ApplyActorNumbersAtStart: [" + string.Join(", ", actorNumbersAtStart) + "]");
		m_PhotonActorNumbersAtStart.Clear();
		m_PhotonActorNumbersAtStart.AddRange(actorNumbersAtStart);
		if (m_LoadBalancingClient.CurrentRoom == null)
		{
			throw new Exception("LoadBalancingClient.CurrentRoom is null!");
		}
		OtherReceivers.TargetActors = GetTargetActors(actorNumbersAtStart, m_LoadBalancingClient.LocalPlayer.ActorNumber);
		List<PlayerInfo> activePlayers2 = m_ActivePlayers;
		ReadonlyList<PlayerInfo> allPlayers2 = AllPlayers;
		GetActivePlayers(activePlayers2, in allPlayers2);
		DLC.CloseRoom(dlcs);
		IPlatformInvite invite = PlatformServices.Platform.Invite;
		foreach (PlayerInfo activePlayer in m_ActivePlayers)
		{
			invite.SetPlayedWith(activePlayer.UserId);
		}
		static void GetActivePlayers(List<PlayerInfo> activePlayers, in ReadonlyList<PlayerInfo> allPlayers)
		{
			activePlayers.Clear();
			activePlayers.IncreaseCapacity(allPlayers.Count);
			foreach (PlayerInfo allPlayer in allPlayers)
			{
				if (allPlayer.IsActive)
				{
					activePlayers.Add(new PlayerInfo(allPlayer, isActive: true));
				}
			}
		}
		static int[] GetTargetActors(PhotonActorNumber[] actorNumbersAtStart, int localPlayerActorNumber)
		{
			int[] array = new int[actorNumbersAtStart.Length - 2];
			int num = 0;
			for (int i = 0; i < actorNumbersAtStart.Length; i++)
			{
				PhotonActorNumber photonActorNumber = actorNumbersAtStart[i];
				if (photonActorNumber.IsValid && photonActorNumber.ActorNumber != localPlayerActorNumber)
				{
					array[num++] = photonActorNumber.ActorNumber;
				}
			}
			return array;
		}
	}

	public TimeSpan ProcessDeltaTime(TimeSpan lastTickTime, TimeSpan deltaTime)
	{
		return m_ProcessDeltaTimeLogic.ProcessDeltaTime(lastTickTime, deltaTime);
	}

	public PhotonActorNumber PlayerToActorNumber(NetPlayer player)
	{
		if (0 > player.Index || player.Index >= m_PhotonActorNumbersAtStart.Count)
		{
			return PhotonActorNumber.Invalid;
		}
		return new PhotonActorNumber(m_PhotonActorNumbersAtStart[player.Index].ActorNumber);
	}

	public int ActorNumberToPlayerIndex(int actorNumber)
	{
		return m_PhotonActorNumbersAtStart.IndexOf(new PhotonActorNumber(actorNumber));
	}

	public MatchmakingCallbacks CreateMatchmakingCallbacks()
	{
		return new MatchmakingCallbacks(m_LoadBalancingClient);
	}

	public Task WaitPlayerLeftRoom(PhotonActorNumber actorNumber)
	{
		return m_PlayerLeavingController.WaitPlayerLeft(actorNumber);
	}

	public bool CreateRoom(EnterRoomParams roomParams)
	{
		return m_LoadBalancingClient.OpCreateRoom(roomParams);
	}

	public void Reset()
	{
		m_ProcessDeltaTimeLogic.Reset();
	}
}
