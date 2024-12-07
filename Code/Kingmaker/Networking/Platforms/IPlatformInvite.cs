using System;
using System.Threading.Tasks;

namespace Kingmaker.Networking.Platforms;

public interface IPlatformInvite : IDisposable
{
	Task ProcessPendingInvites()
	{
		return Task.CompletedTask;
	}

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
