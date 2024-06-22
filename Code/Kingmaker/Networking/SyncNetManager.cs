using System;
using Core.Cheats;
using Kingmaker.Networking.Desync;
using Kingmaker.Networking.Hash;
using Kingmaker.StateHasher;

namespace Kingmaker.Networking;

public class SyncNetManager
{
	private BaseDesyncDetectionStrategy m_DesyncDetectionStrategy;

	public Type StrategyType => m_DesyncDetectionStrategy.GetType();

	public bool HasDesync => m_DesyncDetectionStrategy.HasDesync;

	public bool WasDesync => m_DesyncDetectionStrategy.WasDesync;

	public SyncNetManager()
	{
		m_DesyncDetectionStrategy = new SlidingWindowDesyncDetectionStrategy();
		PFLog.Net.Log("[SyncNetManager] " + m_DesyncDetectionStrategy.GetType().Name + " will be used for desync detection.");
	}

	public void Reset()
	{
		m_DesyncDetectionStrategy.Reset();
	}

	public void OnLeave()
	{
		Reset();
	}

	public void HandleActorsState()
	{
		m_DesyncDetectionStrategy.ReportState();
	}

	public void HandleDesync(int stepIndex, IDesyncHandler handler)
	{
		using StateHasherContext stateHasherContext = StateHasherContext.Request();
		HashableState hashableState = stateHasherContext.GetHashableState();
		DesyncMeta meta = new DesyncMeta(stepIndex, GetExpandedRoomId(), PhotonManager.Instance.ActivePlayers.Count);
		handler.RaiseDesync(hashableState, meta);
	}

	private string GetExpandedRoomId()
	{
		return PhotonManager.Instance.Region + "_" + PhotonManager.Instance.RoomName;
	}

	[Cheat(Name = "net_desync_default", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void ForceDefaultDesyncDetectionStrategy()
	{
		PhotonManager.Sync.m_DesyncDetectionStrategy = new DefaultDesyncDetectionStrategy();
	}
}
