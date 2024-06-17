using System;
using Photon.Realtime;

namespace Kingmaker.Networking.Tools;

public class PhotonTrafficStats
{
	private readonly LoadBalancingClient m_Client;

	public int SendBytes => m_Client.LoadBalancingPeer.TrafficStatsOutgoing.TotalPacketBytes;

	public int SendBytesPerSec => SendBytes / ElapsedSec;

	public int ReceiveBytes => m_Client.LoadBalancingPeer.TrafficStatsIncoming.TotalPacketBytes;

	public int ReceiveBytesPerSec => ReceiveBytes / ElapsedSec;

	private int ElapsedSec => Math.Max((int)(m_Client.LoadBalancingPeer.TrafficStatsElapsedMs / 1000), 1);

	public PhotonTrafficStats(LoadBalancingClient client)
	{
		m_Client = client;
	}

	public void Reset()
	{
		m_Client.LoadBalancingPeer.TrafficStatsReset();
	}
}
