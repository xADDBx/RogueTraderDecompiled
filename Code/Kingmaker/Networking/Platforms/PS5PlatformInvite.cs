using System;

namespace Kingmaker.Networking.Platforms;

public class PS5PlatformInvite : IPlatformInvite, IDisposable
{
	bool IPlatformInvite.TryGetInviteRoom(out string roomServer, out string roomName)
	{
		roomServer = null;
		roomName = null;
		PFLog.Net.Error("[PS5] PSN is disabled!");
		return false;
	}

	public bool IsSupportInviteWindow()
	{
		PFLog.Net.Error("[PS5] PSN is disabled!");
		return false;
	}

	void IPlatformInvite.ShowInviteWindow()
	{
		PFLog.Net.Error("[PS5] PSN is disabled!");
	}

	void IPlatformInvite.Invite(string userId)
	{
		PFLog.Net.Error("[PS5] PSN is disabled!");
	}

	void IPlatformInvite.StartAnnounceGame()
	{
		PFLog.Net.Error("[PS5] PSN is disabled!");
	}

	void IPlatformInvite.StopAnnounceGame()
	{
		PFLog.Net.Error("[PS5] PSN is disabled!");
	}

	public void Dispose()
	{
		PFLog.Net.Error("[PS5] PSN is disabled!");
	}
}
