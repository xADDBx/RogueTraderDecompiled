using System;

namespace Kingmaker.Networking.Platforms;

public sealed class EpicGamesStorePlatformInvite : IPlatformInvite, IDisposable
{
	public bool TryGetInviteRoom(out string roomServer, out string roomName)
	{
		roomServer = (roomName = null);
		PFLog.Net.Error("[EpicGamesStorePlatformInvite] EpicGamesStore is disabled!");
		return false;
	}

	public bool IsSupportInviteWindow()
	{
		PFLog.Net.Error("[EpicGamesStorePlatformInvite] EpicGamesStore is disabled!");
		return false;
	}

	void IPlatformInvite.ShowInviteWindow()
	{
		PFLog.Net.Error("[EpicGamesStorePlatformInvite] EpicGamesStore is disabled!");
	}

	void IPlatformInvite.Invite(string userId)
	{
		PFLog.Net.Error("[EpicGamesStorePlatformInvite] EpicGamesStore is disabled!");
	}

	void IPlatformInvite.StartAnnounceGame()
	{
		PFLog.Net.Error("[EpicGamesStorePlatformInvite] EpicGamesStore is disabled!");
	}

	void IPlatformInvite.StopAnnounceGame()
	{
		PFLog.Net.Error("[EpicGamesStorePlatformInvite] EpicGamesStore is disabled!");
	}

	public void Dispose()
	{
		PFLog.Net.Error("[EpicGamesStorePlatformInvite] EpicGamesStore is disabled!");
	}
}
