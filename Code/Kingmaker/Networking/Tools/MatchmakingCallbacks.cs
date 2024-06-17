using System;
using System.Collections.Generic;
using Photon.Realtime;

namespace Kingmaker.Networking.Tools;

public class MatchmakingCallbacks : IMatchmakingCallbacks, IDisposable
{
	private LoadBalancingClient m_LoadBalancingClient;

	private Action<List<FriendInfo>> m_Friends;

	private Action m_OnCreateRoom;

	private Action<short, string> m_OnCreateRoomFailed;

	private Action m_OnJoinedRoom;

	private Action<short, string> m_OnJoinRoomFailed;

	private Action<short, string> m_OnJoinRandomFailed;

	private Action m_OnLeftRoom;

	public MatchmakingCallbacks(LoadBalancingClient loadBalancingClient)
	{
		m_LoadBalancingClient = loadBalancingClient;
		m_LoadBalancingClient.AddCallbackTarget(this);
	}

	public MatchmakingCallbacks SetLoadBalancingClient(LoadBalancingClient client)
	{
		m_LoadBalancingClient = client;
		return this;
	}

	public MatchmakingCallbacks SetFriendsCallback(Action<List<FriendInfo>> callback)
	{
		m_Friends = callback;
		return this;
	}

	public MatchmakingCallbacks SetOnCreateRoomCallback(Action callback)
	{
		m_OnCreateRoom = callback;
		return this;
	}

	public MatchmakingCallbacks SetOnCreateRoomFailedCallback(Action<short, string> callback)
	{
		m_OnCreateRoomFailed = callback;
		return this;
	}

	public MatchmakingCallbacks SetOnJoinedRoomCallback(Action callback)
	{
		m_OnJoinedRoom = callback;
		return this;
	}

	public MatchmakingCallbacks SetOnJoinRoomFailedCallback(Action<short, string> callback)
	{
		m_OnJoinRoomFailed = callback;
		return this;
	}

	public MatchmakingCallbacks SetOnJoinRandomFailedCallback(Action<short, string> callback)
	{
		m_OnJoinRandomFailed = callback;
		return this;
	}

	public MatchmakingCallbacks SetOnLeftRoomCallback(Action callback)
	{
		m_OnLeftRoom = callback;
		return this;
	}

	void IMatchmakingCallbacks.OnFriendListUpdate(List<FriendInfo> friendList)
	{
		m_Friends?.Invoke(friendList);
	}

	void IMatchmakingCallbacks.OnCreatedRoom()
	{
		m_OnCreateRoom?.Invoke();
	}

	void IMatchmakingCallbacks.OnCreateRoomFailed(short returnCode, string message)
	{
		m_OnCreateRoomFailed?.Invoke(returnCode, message);
	}

	void IMatchmakingCallbacks.OnJoinedRoom()
	{
		m_OnJoinedRoom?.Invoke();
	}

	void IMatchmakingCallbacks.OnJoinRoomFailed(short returnCode, string message)
	{
		m_OnJoinRoomFailed?.Invoke(returnCode, message);
	}

	void IMatchmakingCallbacks.OnJoinRandomFailed(short returnCode, string message)
	{
		m_OnJoinRandomFailed?.Invoke(returnCode, message);
	}

	void IMatchmakingCallbacks.OnLeftRoom()
	{
		m_OnLeftRoom?.Invoke();
	}

	public void Dispose()
	{
		m_LoadBalancingClient.RemoveCallbackTarget(this);
	}
}
