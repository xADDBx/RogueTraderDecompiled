using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Kingmaker.Networking;

public class PhotonNetworkStats
{
	private readonly LoadBalancingClient m_Client;

	public int Rtt => Peer.RoundTripTime;

	public int RttVariance => Peer.RoundTripTimeVariance;

	public int LastSocketReceive => Peer.ConnectionTime - Peer.TimestampOfLastSocketReceive;

	public int LongestDeltaBetweenDispatching => TrafficStats.LongestDeltaBetweenDispatching;

	public int LongestDeltaBetweenSending => TrafficStats.LongestDeltaBetweenSending;

	private LoadBalancingPeer Peer => m_Client.LoadBalancingPeer;

	private TrafficStatsGameLevel TrafficStats => Peer.TrafficStatsGameLevel;

	public PhotonNetworkStats(LoadBalancingClient client)
	{
		m_Client = client;
	}

	public string GetLongestOperationsStats()
	{
		TrafficStatsGameLevel trafficStats = TrafficStats;
		return $"OnEv=[{trafficStats.LongestEventCallbackCode}]/{trafficStats.LongestEventCallback} OnResp=[{trafficStats.LongestOpResponseCallbackOpCode}]/{trafficStats.LongestOpResponseCallback}";
	}
}
