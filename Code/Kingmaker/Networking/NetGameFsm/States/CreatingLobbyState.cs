using System;
using System.Threading.Tasks;
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

	private void RunCreateRoom()
	{
		LobbyNetManager.SetState(LobbyNetManager.State.Connecting);
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
					MaxPlayers = 6,
					PublishUserId = true
				}
			};
		}
	}

	private void OnCreateRoom()
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
		EventBus.RaiseEvent(delegate(INetLobbyErrorHandler h)
		{
			h.HandleCreatingLobbyError(returnCode);
		});
	}
}
