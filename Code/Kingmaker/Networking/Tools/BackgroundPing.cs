using System;
using System.Diagnostics;
using System.Threading;
using Photon.Realtime;

namespace Kingmaker.Networking.Tools;

public class BackgroundPing : IDisposable
{
	private static readonly TimeSpan PingInterval = TimeSpan.FromMilliseconds(3000.0);

	private readonly Timer m_Timer;

	private readonly LoadBalancingClient m_LoadBalancingClient;

	private readonly Stopwatch m_Stopwatch;

	public BackgroundPing(LoadBalancingClient loadBalancingClient)
	{
		m_LoadBalancingClient = loadBalancingClient;
		m_Timer = new Timer(OnTime, null, PingInterval, PingInterval);
		m_Stopwatch = new Stopwatch();
	}

	public void ResetTime()
	{
		m_Timer.Change(PingInterval, PingInterval);
	}

	private void OnTime(object state)
	{
		m_Stopwatch.Restart();
		m_LoadBalancingClient.LoadBalancingPeer.SendAcksOnly();
		m_Stopwatch.Stop();
		PFLog.Net.Log($"[BackgroundPing.OnTime] elapsed {m_Stopwatch.ElapsedMilliseconds} ms");
	}

	public void Dispose()
	{
		m_Timer.Dispose();
	}
}
