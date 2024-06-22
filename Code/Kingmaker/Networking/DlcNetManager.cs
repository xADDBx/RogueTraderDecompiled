using System.Collections.Generic;
using System.Linq;
using Code.GameCore.Mics;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Networking.Player;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.Utility.DotNetExtensions;
using Photon.Realtime;

namespace Kingmaker.Networking;

public sealed class DlcNetManager
{
	public class LocalPlayerDlcCheck : ContextFlag<LocalPlayerDlcCheck>
	{
	}

	private readonly Dictionary<string, string[]> m_UserIdToDLC = new Dictionary<string, string[]>();

	private readonly List<string> m_DLCsInLobbyCache = new List<string>();

	private readonly List<string> m_DLCsInGameCache = new List<string>();

	private readonly List<string> m_RoomOwnerDLCsCache = new List<string>();

	public bool IsDLCsInLobbyReady
	{
		get
		{
			ReadonlyList<PlayerInfo> allPlayers = PhotonManager.Instance.AllPlayers;
			NetPlayer roomOwner = PhotonManager.Instance.RoomOwner;
			foreach (PlayerInfo item in allPlayers)
			{
				if (item.NetPlayer.Index == roomOwner.Index && !m_UserIdToDLC.ContainsKey(item.UserId))
				{
					return false;
				}
			}
			return true;
		}
	}

	private ReadonlyList<string> RoomOwnerDLCs
	{
		get
		{
			m_RoomOwnerDLCsCache.Clear();
			ReadonlyList<PlayerInfo> allPlayers = PhotonManager.Instance.AllPlayers;
			NetPlayer roomOwner = PhotonManager.Instance.RoomOwner;
			foreach (PlayerInfo item in allPlayers)
			{
				if (item.NetPlayer.Index == roomOwner.Index)
				{
					if (m_UserIdToDLC.TryGetValue(item.UserId, out var value))
					{
						m_RoomOwnerDLCsCache.AddRange(value);
					}
					break;
				}
			}
			return m_RoomOwnerDLCsCache.ToArray();
		}
	}

	public ReadonlyList<string> DLCsInConnect
	{
		get
		{
			UpdateCache(m_DLCsInLobbyCache, RoomOwnerDLCs);
			return m_DLCsInLobbyCache;
			static void UpdateCache(List<string> dlc, IEnumerable<string> roomOwnerDLCs)
			{
				dlc.Clear();
				dlc.AddRange(roomOwnerDLCs);
			}
		}
	}

	public ReadonlyList<string> DLCsInGame => m_DLCsInGameCache;

	private static string[] LocalPlayerDLCs
	{
		get
		{
			using (ContextData<LocalPlayerDlcCheck>.Request())
			{
				return (from dlc in InterfaceServiceLocator.TryGetService<IDlcRootService>()?.Dlcs
					where dlc.IsAvailable
					select dlc.Id).ToArray();
			}
		}
	}

	public bool TryGetPlayerDLC(string userId, out List<IBlueprintDlc> playerDLCs)
	{
		if (m_UserIdToDLC.TryGetValue(userId, out var dlc))
		{
			playerDLCs = InterfaceServiceLocator.TryGetService<IDlcRootService>()?.Dlcs.Where((IBlueprintDlc x) => dlc.Contains(x.Id)).ToList();
			return true;
		}
		playerDLCs = null;
		return false;
	}

	public void OnJoinedLobby()
	{
		PFLog.Net.Log("[DLC] Uploading DLC list...");
		string[] localPlayerDLCs = LocalPlayerDLCs;
		PhotonManager.Instance.SetPlayerProperty("d", localPlayerDLCs);
		PFLog.Net.Log($"[DLC] Available DLCs x{localPlayerDLCs.Length}");
		int i = 0;
		for (int num = localPlayerDLCs.Length; i < num; i++)
		{
			PFLog.Net.Log($"[DLC]   {i}) {localPlayerDLCs[i]}");
		}
		foreach (PlayerInfo allPlayer in PhotonManager.Instance.AllPlayers)
		{
			if (PhotonManager.Instance.ActorNumberToPhotonPlayer(allPlayer.Player, out var player))
			{
				OnPlayerUpdate(player);
			}
		}
	}

	public void OnPlayerUpdate(Photon.Realtime.Player photonPlayer)
	{
		if (PhotonManager.Instance.GetPlayerProperty<string[]>(photonPlayer, "d", out var obj))
		{
			m_UserIdToDLC[photonPlayer.UserId] = obj;
			PFLog.Net.Log($"[DLC] OnPlayerUpdate {photonPlayer.UserId} x{obj.Length}");
			int i = 0;
			for (int num = obj.Length; i < num; i++)
			{
				PFLog.Net.Log($"[DLC]   {i}) {obj[i]}");
			}
			LogDLCsThatEveryoneHas();
			EventBus.RaiseEvent(delegate(INetDLCsHandler h)
			{
				h.HandleDLCsListChanged();
			});
		}
	}

	public void OnPlayerLeftRoom(Photon.Realtime.Player photonPlayer)
	{
		PFLog.Net.Log("[DLC] OnPlayerLeftRoom " + photonPlayer.UserId);
		m_UserIdToDLC.Remove(photonPlayer.UserId);
		LogDLCsThatEveryoneHas();
		EventBus.RaiseEvent(delegate(INetDLCsHandler h)
		{
			h.HandleDLCsListChanged();
		});
	}

	public string[] CreateDlcInGameList()
	{
		return RoomOwnerDLCs.ToArray();
	}

	public void CloseRoom(string[] dlcs)
	{
		m_DLCsInGameCache.Clear();
		m_DLCsInGameCache.AddRange(dlcs);
		PFLog.Net.Log($"[DLC] DLCsInGame x{m_DLCsInGameCache.Count}");
		int i = 0;
		for (int count = m_DLCsInGameCache.Count; i < count; i++)
		{
			PFLog.Net.Log($"[DLC]   {i}) {m_DLCsInGameCache[i]}");
		}
	}

	public void OnLeave()
	{
		m_UserIdToDLC.Clear();
		m_DLCsInLobbyCache.Clear();
		m_DLCsInGameCache.Clear();
		m_RoomOwnerDLCsCache.Clear();
	}

	private void LogDLCsThatEveryoneHas()
	{
		ReadonlyList<string> dLCsInConnect = DLCsInConnect;
		PFLog.Net.Log($"[DLC] DLCsInLobby x{dLCsInConnect.Count}");
		int i = 0;
		for (int count = dLCsInConnect.Count; i < count; i++)
		{
			PFLog.Net.Log($"[DLC]   {i}) {dLCsInConnect[i]}");
		}
	}
}
