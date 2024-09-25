using System;
using Galaxy.Api;
using Kingmaker.Utility.CommandLineArgs;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Networking.Platforms;

public class GoGPlatformInvite : IPlatformInvite, IDisposable
{
	private class RichPresenceListener : IRichPresenceChangeListener
	{
		private readonly string m_Key;

		private readonly string m_Value;

		public RichPresenceListener(string key, string value)
		{
			m_Key = key;
			m_Value = value;
		}

		public override void OnRichPresenceChangeSuccess()
		{
			PFLog.Net.Log("[GoGPlatformInvite] SetRichPresence(" + m_Key + ", " + m_Value + ") done!");
		}

		public override void OnRichPresenceChangeFailure(FailureReason failureReason)
		{
			PFLog.Net.Error("[GoGPlatformInvite] SetRichPresence(" + m_Key + ", " + m_Value + ") failed!");
		}
	}

	private class GameJoinRequestedListener : GlobalGameJoinRequestedListener
	{
		public override void OnGameJoinRequested(GalaxyID userID, string connectionString)
		{
			PFLog.Net.Log($"[GameJoinRequestedListener] OnGameJoinRequested '{userID}' '{connectionString}'");
			if (ParseString(connectionString, out var server, out var room, hasPrefix: true))
			{
				PhotonManager.Invite.AcceptInvite(server, room);
			}
		}
	}

	private const string Prefix = "--JoinLobby";

	private const string PrefixSeparator = "=";

	private const string Separator = ":";

	private readonly IDisposable m_Listener = new GameJoinRequestedListener();

	private static string Region => PhotonManager.Instance.Region;

	private static string RoomName => PhotonManager.Instance.RoomName;

	public bool TryGetInviteRoom(out string roomServer, out string roomName)
	{
		CommandLineArguments commandLineArguments = CommandLineArguments.Parse();
		PFLog.Net.Log("[TryGetInviteRoom] CommandLineArguments='" + string.Join(',', Environment.GetCommandLineArgs()) + "'");
		if (!commandLineArguments.Contains("--JoinLobby"))
		{
			roomServer = null;
			roomName = null;
			return false;
		}
		return ParseString(commandLineArguments.Get("--JoinLobby"), out roomServer, out roomName, hasPrefix: false);
	}

	public bool IsSupportInviteWindow()
	{
		return true;
	}

	public void ShowInviteWindow()
	{
		if (!FormatString(Region, RoomName, out var output))
		{
			return;
		}
		PFLog.Net.Log("Trying to open overlay invite dialog");
		try
		{
			GalaxyInstance.Friends().ShowOverlayInviteDialog(output);
			PFLog.Net.Log("Showing Galaxy overlay invite dialog");
		}
		catch (Exception ex)
		{
			PFLog.Net.Warning("Could not show Galaxy overlay invite dialog for reason: " + ex);
		}
	}

	public void Invite(string userId)
	{
		if (!ulong.TryParse(userId, out var result))
		{
			throw new Exception("UserId='" + userId + "'");
		}
		GalaxyID galaxyID = new GalaxyID(result);
		if (!FormatString(Region, RoomName, out var output))
		{
			return;
		}
		PFLog.Net.Log("Trying to send invitation to " + galaxyID?.ToString() + " Connection string: " + output);
		try
		{
			GalaxyInstance.Friends().SendInvitation(galaxyID, output);
			PFLog.Net.Log("Sent invitation to: " + userId + " Connection string: " + output);
		}
		catch (Exception ex)
		{
			PFLog.Net.Warning("Could not send invitation to: " + galaxyID?.ToString() + " Connection string: " + output + " for reason: " + ex);
		}
	}

	void IPlatformInvite.StartAnnounceGame()
	{
		if (FormatString(Region, RoomName, out var output))
		{
			GalaxyInstance.Friends().SetRichPresence("connect", output, new RichPresenceListener("connect", output));
		}
	}

	void IPlatformInvite.StopAnnounceGame()
	{
		GalaxyInstance.Friends().ClearRichPresence();
	}

	public void Dispose()
	{
	}

	private static bool ParseString(string input, out string server, out string room, bool hasPrefix)
	{
		if (string.IsNullOrEmpty(input))
		{
			PFLog.Net.Log("[GoGPlatformInvite] ParseString failed! input is null or empty");
			server = null;
			room = null;
			return false;
		}
		if (hasPrefix)
		{
			string[] array = input.Split("=");
			int num = array.IndexOf("--JoinLobby");
			if (num == -1)
			{
				PFLog.Net.Log("[GoGPlatformInvite] ParseString failed! Prefix not found! input='" + input + "' prefix='--JoinLobby'");
				server = null;
				room = null;
				return false;
			}
			if (!array.TryGet(num + 1, out var element) || string.IsNullOrEmpty(element))
			{
				PFLog.Net.Error($"[GoGPlatformInvite] ParseString failed! connectParams is null or empty found! input='{input}' i={num}");
				server = null;
				room = null;
				return false;
			}
			input = element;
		}
		string[] array2 = input.Split(":");
		if (array2.Length != 2)
		{
			PFLog.Net.Error($"[GoGPlatformInvite] ParseString failed! Unexpected input elements count={array2.Length}! input='{input}'");
			server = null;
			room = null;
			return false;
		}
		server = array2[0];
		room = array2[1];
		if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(room))
		{
			PFLog.Net.Error("[GoGPlatformInvite] ParseString failed! Server or room is null or empty! input='" + input + "'");
			server = null;
			room = null;
			return false;
		}
		return true;
	}

	private static bool FormatString(string server, string room, out string output)
	{
		if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(room))
		{
			PFLog.Net.Error("[GoGPlatformInvite] FormatString failed! server='" + server + "' room='" + room + "'");
			output = null;
			return false;
		}
		output = "--JoinLobby=" + server + ":" + room;
		return true;
	}
}
