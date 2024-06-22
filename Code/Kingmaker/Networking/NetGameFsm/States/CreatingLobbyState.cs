using System;
using System.Threading.Tasks;
using Kingmaker.Networking.Platforms;
using Kingmaker.Networking.Platforms.Session;
using Kingmaker.Networking.Settings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Fsm;
using Photon.Realtime;

namespace Kingmaker.Networking.NetGameFsm.States;

public class CreatingLobbyState : IStateAsync
{
	private readonly INetGame m_NetGame;

	private readonly PhotonManager m_PhotonManager;

	private IDisposable m_Callback;

	public CreatingLobbyState(INetGame netGame, PhotonManager photonManager)
	{
		m_NetGame = netGame;
		m_PhotonManager = photonManager;
	}

	public Task OnEnter()
	{
		m_Callback = m_PhotonManager.CreateMatchmakingCallbacks().SetOnCreateRoomCallback(OnCreateRoom).SetOnCreateRoomFailedCallback(OnCreateRoomFailed);
		RunCreateRoom();
		return Task.CompletedTask;
	}

	public Task OnExit()
	{
		m_Callback.Dispose();
		return Task.CompletedTask;
	}

	private async void RunCreateRoom()
	{
		LobbyNetManager.SetState(LobbyNetManager.State.Connecting);
		var (flag, returnCode) = await PlatformServices.Platform.Session.IsEligibleToPlay();
		if (!flag)
		{
			Fail(returnCode, "Player is not eligible to play Coop");
			return;
		}
		EnterRoomParams enterRoomParams = CreateRoomParams();
		PFLog.Net.Log("Creating room " + enterRoomParams.RoomName);
		if (!m_PhotonManager.CreateRoom(enterRoomParams))
		{
			Fail(-2, "Can't send room creation");
		}
		static EnterRoomParams CreateRoomParams()
		{
			return new EnterRoomParams
			{
				RoomName = NetRoomNameHelper.GenerateRoomName(),
				RoomOptions = new RoomOptions
				{
					MaxPlayers = PhotonManager.MaxPlayers,
					PublishUserId = true
				}
			};
		}
	}

	private async void OnCreateRoom()
	{
		if (!PlatformServices.Platform.Session.IsPlatformSessionRequired())
		{
			EnterLobby();
			return;
		}
		PFLog.Net.Log("[CreatingLobbyState] Creating platform session");
		if (!NetRoomNameHelper.TryFormatString(PhotonManager.Instance.Region, PhotonManager.Instance.RoomName, out var output))
		{
			PFLog.Net.Error("[CreatingLobbyState] Failed to create platform session. No room details provided");
			return;
		}
		SessionCreationParams sessionCreationParams = default(SessionCreationParams);
		sessionCreationParams.CoopRoomDetails = output;
		sessionCreationParams.InviteRights = m_NetGame.CurrentInvitableUserType;
		sessionCreationParams.JoinRights = m_NetGame.CurrentJoinableUserType;
		SessionCreationParams sessionParams = sessionCreationParams;
		if (!(await PlatformServices.Platform.Session.CreateSession(sessionParams)))
		{
			PFLog.Net.Log("[CreatingLobbyState] Creating platform session");
			Fail(-1, "Can't create platform session");
		}
		else
		{
			EnterLobby();
		}
	}

	private void EnterLobby()
	{
		LobbyNetManager.SetState(LobbyNetManager.State.InLobby);
		PFLog.Net.Log("Lobby created " + PhotonManager.Instance.RoomName);
		PFLog.Net.Log("Uploading settings...");
		BaseSettingNetData[] data = PhotonManager.Settings.CollectState();
		PhotonManager.Instance.SetRoomProperty("st", data);
		PFLog.Net.Log("Settings uploaded!");
		PhotonManager.Invite.StartAnnounceGame();
		m_NetGame.OnLobbyCreated();
	}

	private void OnCreateRoomFailed(short returnCode, string message)
	{
		PFLog.Net.Log($"PhotonManager.OnCreateRoomFailed({returnCode}, {message})");
		if (returnCode == 32766)
		{
			RunCreateRoom();
		}
		else
		{
			Fail(returnCode, message);
		}
	}

	private void Fail(short returnCode, string message)
	{
		PFLog.Net.Error($"[CreatingLobbyState.Fail] code={returnCode}, msg={message}");
		m_NetGame.OnLobbyCreationFailed();
		if (returnCode == 3000)
		{
			EventBus.RaiseEvent(delegate(INetLobbyErrorHandler h)
			{
				h.HandleNoPlayStationPlusError();
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(INetLobbyErrorHandler h)
			{
				h.HandleCreatingLobbyError(returnCode);
			});
		}
	}
}
