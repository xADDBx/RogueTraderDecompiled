using System;

namespace Kingmaker.Networking.Platforms;

public interface IPlatformInvite : IDisposable
{
	bool TryGetInviteRoom(out string roomServer, out string roomName);

	bool IsSupportInviteWindow();

	void ShowInviteWindow();

	void Invite(string userId);

	void StartAnnounceGame();

	void StopAnnounceGame();

	void SetPlayedWith(string userId)
	{
	}
}
