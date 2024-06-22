using Steamworks;

namespace Kingmaker.Utility;

public static class ApplicationHelper
{
	public static bool IsRunOnSteamDeck
	{
		get
		{
			if (SteamManager.Initialized)
			{
				return SteamUtils.IsSteamRunningOnSteamDeck();
			}
			return false;
		}
	}
}
