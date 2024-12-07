using System;
using System.Globalization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.Networking.Platforms;
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

	public async Task OnEnter()
	{
		LobbyNetManager.SetState(LobbyNetManager.State.Connecting);
		var (flag, returnCode) = await PlatformServices.Platform.Session.IsEligibleToPlay();
		if (!flag)
		{
			OnFailed(returnCode, "Player is not eligible to play Coop");
			return;
		}
		m_Callback = PhotonManager.Instance.CreateMatchmakingCallbacks().SetOnJoinedRoomCallback(OnJoinedRoom).SetOnJoinRandomFailedCallback(OnFailed)
			.SetOnJoinRoomFailedCallback(OnFailed);
		JoinRoom();
	}

	private void JoinRoom()
	{
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
	}

	private async void OnJoinedRoom()
	{
		if (!PlatformServices.Platform.Session.IsPlatformSessionRequired())
		{
			OnJoined();
			return;
		}
		if (!PhotonManager.Instance.GetRoomProperty<string>("ps", out var obj))
		{
			PFLog.Net.Error("[JoiningLobbyState] Failed to join platform session: session ID not found ");
			m_NetGame.OnLobbyJoinFailed();
			return;
		}
		PFLog.Net.Log("[JoiningLobbyState] Joining platform session: '" + obj + "'...");
		if (!(await PlatformServices.Platform.Session.JoinSession(obj)))
		{
			PFLog.Net.Error("[JoiningLobbyState] failed to join platform session");
			OnFailed(-1, "Failed to join platform session");
		}
		else
		{
			OnJoined();
		}
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
		switch (returnCode)
		{
		case 3000:
			EventBus.RaiseEvent(delegate(INetLobbyErrorHandler h)
			{
				h.HandleNoPlayStationPlusError();
			});
			break;
		case 3001:
			EventBus.RaiseEvent(delegate(INetLobbyErrorHandler h)
			{
				h.HandleUserPermissionsError();
			});
			break;
		case 32758:
			EventBus.RaiseEvent(delegate(INetLobbyErrorHandler h)
			{
				h.HandleLobbyNotFoundError();
			});
			break;
		case 32765:
			EventBus.RaiseEvent(delegate(INetLobbyErrorHandler h)
			{
				h.HandleLobbyFullError();
			});
			break;
		default:
			EventBus.RaiseEvent(delegate(INetLobbyErrorHandler h)
			{
				h.HandleJoinLobbyError(returnCode);
			});
			break;
		}
	}

	public Task OnExit()
	{
		m_Callback?.Dispose();
		return Task.CompletedTask;
	}
}
