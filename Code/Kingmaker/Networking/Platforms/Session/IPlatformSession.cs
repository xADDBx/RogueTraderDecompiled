using System.Threading.Tasks;

namespace Kingmaker.Networking.Platforms.Session;

public interface IPlatformSession
{
	bool IsPlatformSessionRequired();

	Task<(bool, short)> IsEligibleToPlay();

	Task<bool> CreateSession(SessionCreationParams sessionParams);

	Task<bool> JoinSession(string sessionId);

	void OnJoinedLobby();

	void LeaveSession();
}
