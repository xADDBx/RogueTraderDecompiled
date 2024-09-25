using System.Threading.Tasks;

namespace Kingmaker.Networking.Platforms.Session;

public class DummyPlatformSession : IPlatformSession
{
	public Task<(bool, short)> IsEligibleToPlay()
	{
		return Task.FromResult((true, (short)0));
	}

	public bool IsPlatformSessionRequired()
	{
		return false;
	}

	public Task<bool> CreateSession(SessionCreationParams sessionParams)
	{
		return Task.FromResult(result: true);
	}

	public Task<bool> JoinSession(string sessionId)
	{
		return Task.FromResult(result: true);
	}

	public void OnJoinedLobby()
	{
	}

	public void LeaveSession()
	{
	}
}
