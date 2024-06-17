using System;

namespace Kingmaker.Networking.Platforms;

public interface IPlatformInvite : IDisposable
{
	bool TryGetInviteRoom(out string roomServer, out string roomName);

	void ShowInviteWindow();

	void Invite(string userId);

	void StartAnnounceGame();

	void StopAnnounceGame();
}
