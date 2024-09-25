using System;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Networking;

[Obsolete("Use NetGame (via PhotonManager.NetGame) instead")]
public class LobbyNetManager
{
	public enum State
	{
		None,
		Connecting,
		Playing,
		InLobby,
		Loading
	}

	private State m_CurrentState;

	private bool m_FirstLoadCompleted;

	public bool PingPressed;

	public bool IsHost
	{
		get
		{
			if (PhotonManager.Initialized)
			{
				return PhotonManager.Instance.IsRoomOwner;
			}
			return true;
		}
	}

	public State CurrentState
	{
		get
		{
			return m_CurrentState;
		}
		private set
		{
			if (m_CurrentState != value)
			{
				PFLog.Net.Log($"LobbyState {m_CurrentState}->{value}");
				m_CurrentState = value;
				EventBus.RaiseEvent(delegate(INetEvents h)
				{
					h.HandleNetStateChanged(value);
				});
			}
		}
	}

	public bool IsActive => CurrentState != State.None;

	public bool IsPlaying => CurrentState == State.Playing;

	public bool FirstLoadCompleted
	{
		get
		{
			if (CurrentState == State.Playing)
			{
				return m_FirstLoadCompleted;
			}
			return false;
		}
	}

	public bool IsLoading => CurrentState == State.Loading;

	public bool InLobby => CurrentState == State.InLobby;

	public void OnLeave()
	{
		PFLog.Net.Log("[LobbyNetManager.OnLeave]");
		CurrentState = State.None;
		m_FirstLoadCompleted = false;
	}

	[Obsolete("Temporary, until FSM development in progress")]
	public static void SetState(State state)
	{
		PhotonManager.Lobby.CurrentState = state;
	}

	[Obsolete("Temporary, until FSM development in progress")]
	public static void SetFirstLoadCompleted(bool firstLoadCompleted)
	{
		PhotonManager.Lobby.m_FirstLoadCompleted = firstLoadCompleted;
	}
}
