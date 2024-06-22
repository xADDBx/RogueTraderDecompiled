using System;
using Kingmaker.Utility.DotNetExtensions;
using Steamworks;

namespace Kingmaker.Networking.Platforms;

public sealed class SteamPlatformInvite : IPlatformInvite, IDisposable
{
	private const string Prefix = "+connect";

	private const string Separator = "_";

	private Callback<GameRichPresenceJoinRequested_t> m_GameRichPresenceJoinRequestedCallback;

	private static string Region => PhotonManager.Instance.Region;

	private static string RoomName => PhotonManager.Instance.RoomName;

	public SteamPlatformInvite()
	{
		m_GameRichPresenceJoinRequestedCallback = Callback<GameRichPresenceJoinRequested_t>.Create(OnGameRichPresenceJoinRequested);
	}

	private void OnGameRichPresenceJoinRequested(GameRichPresenceJoinRequested_t gameRichPresenceJoinRequestedT)
	{
		if (ParseString(gameRichPresenceJoinRequestedT.m_rgchConnect, out var server, out var room))
		{
			PhotonManager.Invite.AcceptInvite(server, room);
		}
	}

	bool IPlatformInvite.TryGetInviteRoom(out string roomServer, out string roomName)
	{
		string pszCommandLine;
		int launchCommandLine = SteamApps.GetLaunchCommandLine(out pszCommandLine, 260);
		PFLog.Net.Log($"[SteamPlatformInvite.TryGetInviteRoom] x{launchCommandLine} line='{pszCommandLine}'");
		return ParseString(pszCommandLine, out roomServer, out roomName);
	}

	public bool IsSupportInviteWindow()
	{
		return true;
	}

	void IPlatformInvite.ShowInviteWindow()
	{
		if (FormatString(Region, RoomName, out var output))
		{
			SteamFriends.ActivateGameOverlayInviteDialogConnectString(output);
		}
	}

	void IPlatformInvite.Invite(string userId)
	{
		if (!ulong.TryParse(userId, out var result))
		{
			throw new Exception("UserId='" + userId + "'");
		}
		if (FormatString(Region, RoomName, out var output))
		{
			CSteamID cSteamID = (CSteamID)result;
			if (!SteamFriends.InviteUserToGame(cSteamID, output))
			{
				PFLog.Net.Error($"[SteamPlatformInvite] InviteUserToGame({cSteamID}, {output}) failed!");
			}
		}
	}

	void IPlatformInvite.StartAnnounceGame()
	{
		if (FormatString(Region, RoomName, out var output))
		{
			SetRichPresence("connect", output);
		}
		static void SetRichPresence(string key, string value)
		{
			if (SteamFriends.SetRichPresence(key, value))
			{
				PFLog.Net.Log("[SteamPlatformInvite] SetRichPresence(" + key + ", " + value + ") done!");
			}
			else
			{
				PFLog.Net.Error("[SteamPlatformInvite] SetRichPresence(" + key + ", " + value + ") failed!");
			}
		}
	}

	void IPlatformInvite.StopAnnounceGame()
	{
		SteamFriends.ClearRichPresence();
	}

	public void Dispose()
	{
	}

	public void SetPlayedWith(string userId)
	{
		if (ulong.TryParse(userId, out var result))
		{
			SteamFriends.SetPlayedWith(new CSteamID(result));
		}
		else
		{
			PFLog.Net.Error("[SteamPlatformInvite.SetPlayedWith] invalid userId='" + userId + "'");
		}
	}

	private static bool FormatString(string server, string room, out string output)
	{
		if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(room))
		{
			PFLog.Net.Error("[SteamPlatformInvite] FormatString failed! server='" + Region + "' room='" + RoomName + "'");
			output = null;
			return false;
		}
		output = "+connect " + server + "_" + room;
		return true;
	}

	private static bool ParseString(string input, out string server, out string room)
	{
		if (string.IsNullOrEmpty(input))
		{
			PFLog.Net.Log("[SteamPlatformInvite] ParseString failed! input is null or empty");
			server = null;
			room = null;
			return false;
		}
		string[] array = input.Split(' ');
		int num = array.IndexOf("+connect");
		if (num == -1)
		{
			PFLog.Net.Log("[SteamPlatformInvite] ParseString failed! Prefix not found! input='" + input + "' prefix='+connect'");
			server = null;
			room = null;
			return false;
		}
		if (!array.TryGet(num + 1, out var element) || string.IsNullOrEmpty(element))
		{
			PFLog.Net.Error($"[SteamPlatformInvite] ParseString failed! connectParams is null or empty found! input='{input}' i={num}");
			server = null;
			room = null;
			return false;
		}
		string[] array2 = element.Split("_");
		if (array2.Length != 2)
		{
			PFLog.Net.Error($"[SteamPlatformInvite] ParseString failed! Unexpected input elements count={array2.Length}! input='{input}'");
			server = null;
			room = null;
			return false;
		}
		server = array2[0];
		room = array2[1];
		if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(room))
		{
			PFLog.Net.Error("[SteamPlatformInvite] ParseString failed! Server or room is null or empty! input='" + input + "'");
			server = null;
			room = null;
			return false;
		}
		return true;
	}
}
