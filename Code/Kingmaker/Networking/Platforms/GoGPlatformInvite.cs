using System;

namespace Kingmaker.Networking.Platforms;

public class GoGPlatformInvite : IPlatformInvite, IDisposable
{
	bool IPlatformInvite.TryGetInviteRoom(out string roomServer, out string roomName)
	{
		roomServer = null;
		roomName = null;
		PFLog.Net.Error("[GoGPlatformInvite] GoG is disabled!");
		return false;
	}

	public bool IsSupportInviteWindow()
	{
		PFLog.Net.Error("[GoGPlatformInvite] GoG is disabled!");
		return false;
	}

	void IPlatformInvite.ShowInviteWindow()
	{
		PFLog.Net.Error("[GoGPlatformInvite] GoG is disabled!");
	}

	void IPlatformInvite.Invite(string userId)
	{
		PFLog.Net.Error("[GoGPlatformInvite] GoG is disabled!");
	}

	void IPlatformInvite.StartAnnounceGame()
	{
		PFLog.Net.Error("[GoGPlatformInvite] GoG is disabled!");
	}

	void IPlatformInvite.StopAnnounceGame()
	{
		PFLog.Net.Error("[GoGPlatformInvite] GoG is disabled!");
	}

	public void Dispose()
	{
	}
}
