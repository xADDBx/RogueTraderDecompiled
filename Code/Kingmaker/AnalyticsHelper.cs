using System;
using System.Collections.Generic;
using Kingmaker.GameInfo;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Levelup.Obsolete;
using UnityEngine;

namespace Kingmaker;

public static class AnalyticsHelper
{
	public static readonly string GameStatisticKeyString = "GameStatisticKeyString";

	public static bool IsStatisticsEnabled => false;

	public static void SendEvent(string eventName, Dictionary<string, object> data)
	{
		try
		{
			if (Application.isPlaying && IsStatisticsEnabled)
			{
				ForceSendEvent(eventName, data);
			}
		}
		catch (Exception ex)
		{
			PFLog.GameStatistics.Error(ex);
		}
	}

	public static void ForceSendEvent(string eventName, Dictionary<string, object> data)
	{
		if (string.IsNullOrEmpty(eventName) || data == null || data.Count <= 0)
		{
			return;
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		foreach (KeyValuePair<string, object> datum in data)
		{
			if (!string.IsNullOrEmpty(datum.Key) && datum.Value != null)
			{
				dictionary.Add(datum.Key, datum.Value);
			}
		}
		if (dictionary.Count <= 0 || string.IsNullOrEmpty(GameVersion.Revision))
		{
			return;
		}
		dictionary.Add("app", GetRevisionAppType());
		if (Game.HasInstance)
		{
			if (SettingsRoot.Initialized && Game.Instance.Player != null && Game.Instance.Player.MainCharacterEntity != null)
			{
				dictionary.Add("usr", GetPlayerLevels());
			}
			dictionary.Add("pac", GetPacketInfo(eventName));
		}
	}

	private static string GetRevisionAppType()
	{
		int appType = (int)Game.Instance.Statistic.m_AppType;
		try
		{
			string[] array = GameVersion.Revision.Split(' ');
			string arg = array[0];
			if (array.Length >= 3)
			{
				arg = Convert.ToDateTime(array[0] + " " + array[1]).ToString("dd.MM.yy HH:mm");
				if (array.Length == 4)
				{
					arg = arg + " " + array[3];
				}
				arg = arg + " " + array[2].Substring(0, 8);
			}
			return $"{arg} {appType}";
		}
		catch (Exception ex)
		{
			PFLog.GameStatistics.Exception(ex);
		}
		return $"{GameVersion.Revision} {appType}";
	}

	public static string GetPlayerLevels()
	{
		string empty = string.Empty;
		return $"{LevelUpController.GetEffectiveLevel(Game.Instance.Player.MainCharacterEntity)}:" + $"{(int)SettingsRoot.Difficulty.GameDifficulty}:" + Game.Instance.Statistic.m_gameSessionGUID + empty;
	}

	public static string GetPacketInfo(string eventName)
	{
		if (!Game.Instance.Statistic.m_SentPackets.ContainsKey(eventName))
		{
			Game.Instance.Statistic.m_SentPackets[eventName] = 0L;
		}
		Game.Instance.Statistic.m_SentPackets[eventName]++;
		return Game.Instance.Statistic.m_SentPackets[eventName].ToString();
	}
}
