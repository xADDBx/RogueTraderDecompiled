using System.Collections.Generic;
using Kingmaker.Networking.Player;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;

namespace Kingmaker.Networking.Settings;

public class CoopData
{
	[JsonProperty]
	private readonly List<string> m_PlayerIndexToUserId = new List<string>(8);

	[JsonProperty]
	public readonly PlayerRole PlayerRole = new PlayerRole();

	public void PreSave()
	{
	}

	public void PostLoad()
	{
		if (!NetworkingManager.IsActive)
		{
			return;
		}
		ReadonlyList<PlayerInfo> activePlayers = PhotonManager.Instance.ActivePlayers;
		Log(m_PlayerIndexToUserId, activePlayers);
		PlayerRole.PostLoad(m_PlayerIndexToUserId, activePlayers);
		m_PlayerIndexToUserId.Clear();
		foreach (PlayerInfo item in activePlayers)
		{
			int index = item.NetPlayer.Index;
			m_PlayerIndexToUserId.EnsureIndex(index);
			m_PlayerIndexToUserId[index] = item.UserId;
		}
		static void Log(List<string> playerIndexToUserId, ReadonlyList<PlayerInfo> players)
		{
			PFLog.Net.Log($"[CoopData] Previous players x{playerIndexToUserId.TryCount()}");
			for (int i = 0; i < playerIndexToUserId.TryCount(); i++)
			{
				PFLog.Net.Log(string.Format("[CoopData]   {0}) {1}", i, playerIndexToUserId[i] ?? "<NULL>"));
			}
			PFLog.Net.Log($"[CoopData] Current players x{players.Count}");
			for (int j = 0; j < players.Count; j++)
			{
				PFLog.Net.Log(string.Format("[CoopData]   {0}) {1}", players[j].NetPlayer.Index, players[j].UserId ?? "<NULL>"));
			}
		}
	}
}
