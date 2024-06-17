using System;

namespace Kingmaker.Networking.Platforms;

public sealed class DummyPlatformInvite : IPlatformInvite, IDisposable
{
	bool IPlatformInvite.TryGetInviteRoom(out string roomServer, out string roomName)
	{
		roomServer = null;
		roomName = null;
		PFLog.Net.Error("[DummyPlatformInvite] TryGetInviteRoom");
		return false;
	}

	void IPlatformInvite.ShowInviteWindow()
	{
		PFLog.Net.Error("[DummyPlatformInvite] ShowInviteWindow");
	}

	void IPlatformInvite.Invite(string userId)
	{
		PFLog.Net.Error("[DummyPlatformInvite] Invite " + userId);
	}

	void IPlatformInvite.StartAnnounceGame()
	{
		PFLog.Net.Error("[DummyPlatformInvite] StartAnnounceGame");
	}

	void IPlatformInvite.StopAnnounceGame()
	{
		PFLog.Net.Error("[DummyPlatformInvite] StopAnnounceGame");
	}

	public void Dispose()
	{
		PFLog.Net.Error("[DummyPlatformInvite] Dispose");
	}
}
