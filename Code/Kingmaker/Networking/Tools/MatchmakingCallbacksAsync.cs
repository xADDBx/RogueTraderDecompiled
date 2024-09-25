using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Photon.Realtime;

namespace Kingmaker.Networking.Tools;

public class MatchmakingCallbacksAsync : IMatchmakingCallbacks, IDisposable
{
	private readonly LoadBalancingClient m_LoadBalancingClient;

	private TaskCompletionSource<bool> m_LeaveRoomTcs;

	public MatchmakingCallbacksAsync(LoadBalancingClient loadBalancingClient)
	{
		m_LoadBalancingClient = loadBalancingClient;
		m_LoadBalancingClient.AddCallbackTarget(this);
	}

	public Task WaitLeaveRoom()
	{
		if (m_LeaveRoomTcs != null && !m_LeaveRoomTcs.Task.IsCompleted)
		{
			throw new AlreadyInProgressException();
		}
		m_LeaveRoomTcs = new TaskCompletionSource<bool>();
		return m_LeaveRoomTcs.Task;
	}

	public void Dispose()
	{
		m_LoadBalancingClient.RemoveCallbackTarget(this);
	}

	void IMatchmakingCallbacks.OnFriendListUpdate(List<FriendInfo> friendList)
	{
	}

	void IMatchmakingCallbacks.OnCreatedRoom()
	{
	}

	void IMatchmakingCallbacks.OnCreateRoomFailed(short returnCode, string message)
	{
	}

	void IMatchmakingCallbacks.OnJoinedRoom()
	{
	}

	void IMatchmakingCallbacks.OnJoinRoomFailed(short returnCode, string message)
	{
	}

	void IMatchmakingCallbacks.OnJoinRandomFailed(short returnCode, string message)
	{
	}

	void IMatchmakingCallbacks.OnLeftRoom()
	{
		m_LeaveRoomTcs?.TrySetResult(result: true);
	}
}
