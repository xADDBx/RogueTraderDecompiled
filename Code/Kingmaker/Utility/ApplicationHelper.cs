using Kingmaker.Stores;
using Steamworks;
using UnityEngine;

namespace Kingmaker.Utility;

public static class ApplicationHelper
{
	public static bool IsRunOnSteamDeck
	{
		get
		{
			if (StoreManager.Store == StoreType.Steam && SteamManager.Initialized)
			{
				return SteamUtils.IsSteamRunningOnSteamDeck();
			}
			return false;
		}
	}

	public static string RunningPlatform
	{
		get
		{
			if (!IsRunOnSteamDeck)
			{
				return Application.platform.ToString();
			}
			return "SteamDeck";
		}
	}
}
