using Kingmaker.Stores;
using Steamworks;

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
}
