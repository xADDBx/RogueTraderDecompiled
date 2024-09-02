using Kingmaker.Networking.Platforms.Authentication;
using Kingmaker.Networking.Platforms.Session;
using Kingmaker.Networking.Platforms.User;
using Kingmaker.Stores;

namespace Kingmaker.Networking.Platforms;

public static class PlatformServices
{
	public static Platform Platform { get; } = CreatePlatform(StoreManager.Store, primaryPlatform: true);


	public static Platform CreatePlatform(StoreType store, bool primaryPlatform)
	{
		if (store == StoreType.Steam)
		{
			return new SteamPlatform();
		}
		return new DummyPlatform();
	}

	public static IAuthenticationService CreateAuthService(StoreType store)
	{
		if (store == StoreType.Steam)
		{
			return new SteamAuthenticationService();
		}
		return new NotImplementedAuthenticationService(store);
	}

	public static IPlatformUser CreatePlatformUser(StoreType store)
	{
		if (store == StoreType.Steam)
		{
			return new SteamPlatformUser();
		}
		return new DummyPlatformUser();
	}

	public static IPlatformSession CreatePlatformSession(StoreType store)
	{
		return new DummyPlatformSession();
	}
}
