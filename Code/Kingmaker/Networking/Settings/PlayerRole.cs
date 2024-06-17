using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.GameCommands;
using Kingmaker.Networking.Player;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using UnityEngine.Pool;

namespace Kingmaker.Networking.Settings;

public sealed class PlayerRole
{
	[JsonProperty]
	private readonly Dictionary<string, NetPlayerGroup> m_EntityRoles = new Dictionary<string, NetPlayerGroup>(8);

	public void PostLoad(List<string> oldPlayerIndexToUserId, ReadonlyList<PlayerInfo> newPlayerInfos)
	{
		NetPlayer roomOwner = PhotonManager.Instance.RoomOwner;
		List<int> list = CollectionPool<List<int>, int>.Get();
		list.IncreaseCapacity(oldPlayerIndexToUserId.Count);
		foreach (string item in oldPlayerIndexToUserId)
		{
			int num = -1;
			if (!string.IsNullOrEmpty(item))
			{
				foreach (PlayerInfo item2 in newPlayerInfos)
				{
					if (item.Equals(item2.UserId, StringComparison.Ordinal))
					{
						num = item2.NetPlayer.Index;
						break;
					}
				}
			}
			PFLog.Net.Log($"[PlayerRole] user '{item}' {list.Count}->{num}");
			list.Add(num);
		}
		FixRoles<string>(m_EntityRoles, list, roomOwner);
		CollectionPool<List<int>, int>.Release(list);
		static void FixRoles<T>(Dictionary<T, NetPlayerGroup> roles, List<int> oldIndexToNewIndex, NetPlayer roomOwnerPlayer)
		{
			T[] array = roles.Keys.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				T key = array[i];
				NetPlayerGroup netPlayerGroup = roles[key];
				NetPlayerGroup value = NetPlayerGroup.Offline;
				for (int j = 1; j < oldIndexToNewIndex.Count; j++)
				{
					int num2 = oldIndexToNewIndex[j];
					NetPlayer player = new NetPlayer(j);
					if (netPlayerGroup.Contains(player))
					{
						NetPlayer player2 = ((num2 == -1) ? roomOwnerPlayer : new NetPlayer(num2));
						value = value.Add(player2).Add(NetPlayer.Offline);
						PFLog.Net.Log($"[PlayerRole] fix role '{key.ToString()}' {j}->{player2.Index}");
					}
				}
				if (value.Equals(NetPlayerGroup.Offline))
				{
					value = value.Add(roomOwnerPlayer);
				}
				roles[key] = value;
			}
		}
	}

	public bool Can(string entityId, NetPlayer player)
	{
		if (m_EntityRoles.TryGetValue(entityId, out var value))
		{
			NetPlayerGroup playersReadyMask = PhotonManager.Instance.PlayersReadyMask;
			bool flag = value.Contains(player);
			if (flag)
			{
				flag = playersReadyMask.Contains(player);
			}
			else if (value.Intersection(playersReadyMask).Del(NetPlayerGroup.Offline).IsEmpty)
			{
				flag = NetworkingManager.GameOwner.Equals(player);
			}
			return flag;
		}
		if (NetworkingManager.IsGameOwner)
		{
			Set(entityId, NetworkingManager.GameOwner, enable: true);
			ForceSet(entityId, NetworkingManager.GameOwner, enable: true);
		}
		return player.Equals(NetworkingManager.GameOwner);
	}

	public bool PlayerContainsAnyRole(NetPlayer player)
	{
		return m_EntityRoles.Values.Any((NetPlayerGroup v) => v.Contains(player));
	}

	public void Set(string entityId, NetPlayer player, bool enable)
	{
		Game.Instance.GameCommandQueue.ChangePlayerRole(entityId, player, enable);
	}

	public void ForceSet(string entityId, NetPlayer player, bool enable)
	{
		if (ForceSet(m_EntityRoles, entityId, player, enable))
		{
			EventBus.RaiseEvent(delegate(INetRoleSetHandler h)
			{
				h.HandleRoleSet(entityId);
			});
		}
	}

	private static bool ForceSet<T>(Dictionary<T, NetPlayerGroup> dict, T key, NetPlayer player, bool enable)
	{
		if (!dict.TryGetValue(key, out var value))
		{
			value = NetPlayerGroup.Offline;
			dict.Add(key, value);
		}
		value = NetPlayerGroup.Offline;
		NetPlayerGroup other = (dict[key] = (enable ? value.Add(player) : value.Del(player)).Add(NetPlayer.Offline));
		return !value.Equals(other);
	}
}
