using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Photon.Realtime;

namespace Kingmaker.Networking.Tools;

public class ConnectionCallbacksAsync : IConnectionCallbacks, IDisposable
{
	private readonly LoadBalancingClient m_LoadBalancingClient;

	private TaskCompletionSource<bool> m_ConnectedToMasterTcs;

	private TaskCompletionSource<bool> m_ReconnectTcs;

	public ConnectionCallbacksAsync(LoadBalancingClient loadBalancingClient)
	{
		m_LoadBalancingClient = loadBalancingClient;
		m_LoadBalancingClient.AddCallbackTarget(this);
	}

	public Task WaitConnect()
	{
		if (m_ConnectedToMasterTcs != null && !m_ConnectedToMasterTcs.Task.IsCompleted)
		{
			throw new AlreadyInProgressException();
		}
		m_ConnectedToMasterTcs = new TaskCompletionSource<bool>();
		return m_ConnectedToMasterTcs.Task;
	}

	public Task WaitReconnect()
	{
		if (m_ReconnectTcs != null && !m_ReconnectTcs.Task.IsCompleted)
		{
			throw new AlreadyInProgressException();
		}
		m_ReconnectTcs = new TaskCompletionSource<bool>();
		return m_ReconnectTcs.Task;
	}

	public void Dispose()
	{
		m_LoadBalancingClient.RemoveCallbackTarget(this);
	}

	void IConnectionCallbacks.OnConnected()
	{
	}

	void IConnectionCallbacks.OnConnectedToMaster()
	{
		m_ConnectedToMasterTcs?.TrySetResult(result: true);
		m_ReconnectTcs?.TrySetResult(result: true);
	}

	void IConnectionCallbacks.OnDisconnected(DisconnectCause cause)
	{
		m_ConnectedToMasterTcs?.TrySetException(new PhotonDisconnectedException(cause));
		if (cause != 0)
		{
			m_ReconnectTcs?.TrySetException(new PhotonDisconnectedException(cause));
		}
	}

	void IConnectionCallbacks.OnRegionListReceived(RegionHandler regionHandler)
	{
	}

	void IConnectionCallbacks.OnCustomAuthenticationResponse(Dictionary<string, object> data)
	{
	}

	void IConnectionCallbacks.OnCustomAuthenticationFailed(string debugMessage)
	{
		m_ConnectedToMasterTcs?.TrySetException(new PhotonCustomAuthenticationFailedException(debugMessage));
	}
}
