using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Logging;
using Photon.Realtime;
using UnityEngine;

namespace Kingmaker.Networking;

public static class NetRoomNameHelper
{
	private const int MaxServerLength = 4;

	private const int RoomNameLength = 9;

	private const int MaxCodeLength = 13;

	private const long MaxRandom = 8191L;

	private const string Symbols = "023456789abcdefghijkmnopqrstuvwxyz";

	private static readonly char[] Chars = new char[9];

	public static string GenerateRoomName()
	{
		long num = DateTime.UtcNow.Millisecond % uint.MaxValue;
		long num2 = (long)(UnityEngine.Random.value * 8191f);
		long num3 = num | (num2 << 32);
		int length = "023456789abcdefghijkmnopqrstuvwxyz".Length;
		for (int i = 0; i < 9; i++)
		{
			int index = (int)(num3 % length);
			Chars[i] = "023456789abcdefghijkmnopqrstuvwxyz"[index];
			num3 /= length;
		}
		return new string(Chars);
	}

	public static bool TryFormatString([NotNull] string server, [NotNull] string room, out string output)
	{
		if (string.IsNullOrEmpty(server) || server.Length > 4 || string.IsNullOrEmpty(room))
		{
			PFLog.Net.Error("[NetRoomNameHelper] FormatString failed! server='" + server + "' room='" + room + "'");
			output = null;
			return false;
		}
		output = server.ToLowerInvariant() + room;
		return true;
	}

	public static bool Check([NotNull] string input, [NotNull] List<Region> enabledRegions)
	{
		string server;
		string room;
		return TryParse(input, enabledRegions, null, out server, out room);
	}

	public static bool TryParse([NotNull] string input, [CanBeNull] List<Region> enabledRegions, out string server, out string room)
	{
		return TryParse(input, enabledRegions, PFLog.Net, out server, out room);
	}

	private static bool CheckRoomName(string roomName)
	{
		if (roomName.Length != 9)
		{
			return false;
		}
		for (int i = 0; i < 9; i++)
		{
			if (!"023456789abcdefghijkmnopqrstuvwxyz".Contains(char.ToLowerInvariant(roomName[i])))
			{
				return false;
			}
		}
		return true;
	}

	private static bool CheckRegion([NotNull] string region, [NotNull] List<Region> enabledRegions)
	{
		foreach (Region enabledRegion in enabledRegions)
		{
			if (enabledRegion.Code.Equals(region, StringComparison.Ordinal))
			{
				return true;
			}
		}
		return false;
	}

	private static bool TryParse([NotNull] string input, [CanBeNull] List<Region> enabledRegions, [CanBeNull] LogChannel log, out string server, out string room)
	{
		server = null;
		room = null;
		if (string.IsNullOrEmpty(input))
		{
			log?.Error("[NetRoomNameHelper] ParseString failed! input is null or empty");
			return false;
		}
		int length = input.Length;
		if (length <= 9 || length > 13)
		{
			log?.Error("[NetRoomNameHelper] Invalid code length, code='" + input + "'");
			return false;
		}
		string text = input;
		server = text.Substring(0, text.Length - 9).ToLowerInvariant();
		if (enabledRegions != null && !CheckRegion(server, enabledRegions))
		{
			log?.Error("[NetRoomNameHelper] Invalid region. Input='" + input + ", Server='" + server + "'");
			return false;
		}
		text = input;
		length = server.Length;
		room = text.Substring(length, text.Length - length);
		if (!CheckRoomName(room))
		{
			log?.Error("[NetRoomNameHelper] Invalid room name. Input='" + input + ", Server='" + server + "', Room='" + room + "'");
			return false;
		}
		return true;
	}
}
