using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Networking;

public class LockNetManager
{
	private readonly Dictionary<byte, NetPlayerGroup> m_PointIdToPlayers = new Dictionary<byte, NetPlayerGroup>();

	public bool GetProgress(NetLockPointId pointId, out int current, out int target, out bool me)
	{
		byte key = (byte)pointId;
		if (!m_PointIdToPlayers.TryGetValue(key, out var value))
		{
			current = 0;
			target = 0;
			me = false;
			return false;
		}
		NetPlayerGroup playersReadyMask = NetworkingManager.PlayersReadyMask;
		current = value.Intersection(playersReadyMask).Count();
		target = Mathf.Max(1, playersReadyMask.Count());
		me = value.Contains(PhotonManager.Instance.LocalNetPlayer);
		return true;
	}

	public bool Lock(NetLockPointId pointId)
	{
		NetPlayerGroup playersReadyMask = NetworkingManager.PlayersReadyMask;
		if (AddPlayer(NetworkingManager.LocalNetPlayer, pointId, out var mask))
		{
			SendLockInternal((byte)pointId);
		}
		bool num = mask.Contains(playersReadyMask);
		if (num)
		{
			PFLog.Net.Log($"LockLogic #{pointId} complete!");
			m_PointIdToPlayers.Remove((byte)pointId);
		}
		return num;
	}

	private void Unlock(NetPlayer playerId, NetLockPointId pointId)
	{
		if (!AddPlayer(playerId, pointId, out var _))
		{
			PFLog.Net.Error($"[LockNetManager.Unlock] player={playerId} already unlocked");
		}
	}

	private bool AddPlayer(NetPlayer playerId, NetLockPointId pointId, out NetPlayerGroup mask)
	{
		byte key = (byte)pointId;
		if (!m_PointIdToPlayers.TryGetValue(key, out var value))
		{
			value = NetPlayerGroup.Empty;
			m_PointIdToPlayers.Add(key, value);
		}
		NetPlayerGroup netPlayerGroup = value.Add(playerId);
		bool flag = !value.Equals(netPlayerGroup);
		if (flag)
		{
			m_PointIdToPlayers[key] = netPlayerGroup;
			PFLog.Net.Log($"LockLogic #{pointId} {value.ToString()}->{netPlayerGroup.ToString()} / {NetworkingManager.PlayersReadyMask.ToString()}");
		}
		mask = netPlayerGroup;
		return flag;
	}

	public void OnLockReceived(NetPlayer player, ReadOnlySpan<byte> bytes)
	{
		if (bytes.Length != 1)
		{
			PFLog.Net.Error($"ReadLockMessage: unexpected packet size! {1}/{bytes.Length}");
			return;
		}
		byte pointId = bytes[0];
		Unlock(player, (NetLockPointId)pointId);
	}

	private void SendLockInternal(byte lockPointId)
	{
		if (PhotonManager.Lobby.IsActive)
		{
			byte[] array = MessageNetManager.SendBytes.GetArray();
			array[0] = lockPointId;
			if (!PhotonManager.Instance.SendMessageToOthers(7, array, 0, 1))
			{
				PFLog.Net.Error("Error when trying to send lock!");
			}
		}
	}

	public void OnLeave()
	{
		m_PointIdToPlayers.Clear();
	}
}
