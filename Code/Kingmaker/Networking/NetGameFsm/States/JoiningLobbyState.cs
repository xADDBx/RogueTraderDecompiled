using System;
using System.Globalization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Fsm;

namespace Kingmaker.Networking.NetGameFsm.States;

public class JoiningLobbyState : IStateAsync
{
	private readonly string m_RoomName;

	private readonly INetGame m_NetGame;

	private IDisposable m_Callback;

	public JoiningLobbyState([CanBeNull] string roomName, INetGame netGame)
	{
		m_RoomName = roomName;
		m_NetGame = netGame;
	}

	public Task OnEnter()
	{
		LobbyNetManager.SetState(LobbyNetManager.State.Connecting);
		m_Callback = PhotonManager.Instance.CreateMatchmakingCallbacks().SetOnJoinedRoomCallback(OnJoined).SetOnJoinRandomFailedCallback(OnFailed)
			.SetOnJoinRoomFailedCallback(OnFailed);
		string text = m_RoomName;
		if (!string.IsNullOrEmpty(text))
		{
			text = text.ToLower(CultureInfo.InvariantCulture);
		}
		PFLog.Net.Log("Joining room '" + (text ?? "<NULL>") + "'...");
		if (!(string.IsNullOrEmpty(text) ? PhotonManager.Instance.JoinRandomRoom() : PhotonManager.Instance.JoinRoom(text)))
		{
			PFLog.Net.Error("[JoiningLobbyState.OnEnter] can't send JoinRoom message");
			m_NetGame.OnLobbyJoinFailed();
		}
		return Task.CompletedTask;
	}

	private void OnJoined()
	{
		PFLog.Net.Log("[JoiningRoomState.OnJoined]");
		LobbyNetManager.SetState(LobbyNetManager.State.InLobby);
		PFLog.Net.Log("Connection to Room established.");
		PFLog.Net.Log("Downloading settings...");
		PhotonManager.Settings.OnRoomSettingsUpdate();
		PFLog.Net.Log("Waiting for LoadSave msg...");
		m_NetGame.OnLobbyJoined();
	}

	private void OnFailed(short returnCode, string message)
	{
		PFLog.Net.Error($"[JoiningRoomState.OnFailed] code={returnCode}, msg={message}");
		m_NetGame.OnLobbyJoinFailed();
		if (returnCode == 32758)
		{
			EventBus.RaiseEvent(delegate(INetLobbyErrorHandler h)
			{
				h.HandleLobbyNotFoundError();
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(INetLobbyErrorHandler h)
			{
				h.HandleJoinLobbyError(returnCode);
			});
		}
	}

	public Task OnExit()
	{
		m_Callback.Dispose();
		return Task.CompletedTask;
	}
}
