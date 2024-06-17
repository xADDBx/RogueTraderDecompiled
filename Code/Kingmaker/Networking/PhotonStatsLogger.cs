using System;
using System.Diagnostics;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Kingmaker.Networking;

public class PhotonStatsLogger
{
	private static readonly TimeSpan NormalConnectionLogInterval = TimeSpan.FromMinutes(1.0);

	private static readonly TimeSpan BadConnectionLogInterval = TimeSpan.FromSeconds(3.0);

	private const int BadConnectionTimingMs = 3000;

	private readonly LoadBalancingClient m_Client;

	private readonly Stopwatch m_Stopwatch;

	private bool IsBadConnection
	{
		get
		{
			if (CurrentSendDiff <= 3000)
			{
				return CurrentReceiveDiff > 3000;
			}
			return true;
		}
	}

	private int CurrentSendDiff => m_Client.LoadBalancingPeer.ConnectionTime - m_Client.LoadBalancingPeer.LastSendOutgoingTime;

	private int CurrentReceiveDiff => m_Client.LoadBalancingPeer.ConnectionTime - m_Client.LoadBalancingPeer.TimestampOfLastSocketReceive;

	public PhotonStatsLogger(LoadBalancingClient client)
	{
		m_Client = client;
		m_Stopwatch = Stopwatch.StartNew();
	}

	public void Update()
	{
		if (m_Client.IsConnected && (m_Stopwatch.Elapsed > NormalConnectionLogInterval || (IsBadConnection && m_Stopwatch.Elapsed > BadConnectionLogInterval)))
		{
			TrafficStatsGameLevel trafficStatsGameLevel = m_Client.LoadBalancingPeer.TrafficStatsGameLevel;
			int roundTripTime = m_Client.LoadBalancingPeer.RoundTripTime;
			PFLog.Net.Log($"[LogStats] rtt={roundTripTime} sendDiff={CurrentSendDiff}, recvDiff={CurrentReceiveDiff}, {trafficStatsGameLevel.ToStringVitalStats()}");
			m_Stopwatch.Restart();
		}
	}
}
