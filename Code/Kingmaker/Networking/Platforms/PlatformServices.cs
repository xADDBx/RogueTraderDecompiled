using Kingmaker.Networking.Platforms.Authentication;
using Kingmaker.Networking.Platforms.User;
using Kingmaker.Stores;

namespace Kingmaker.Networking.Platforms;

public static class PlatformServices
{
	public static Platform Platform { get; } = CreatePlatform(StoreManager.Store, primaryPlatform: true);


	public static Platform CreatePlatform(StoreType store, bool primaryPlatform)
	{
		return store switch
		{
			StoreType.Steam => new SteamPlatform(), 
			StoreType.EpicGames => new EpicGamesPlatform(primaryPlatform), 
			StoreType.GoG => new GoGPlatform(), 
			_ => new DummyPlatform(), 
		};
	}

	public static IAuthenticationService CreateAuthService(StoreType store)
	{
		return store switch
		{
			StoreType.Steam => new SteamAuthenticationService(), 
			StoreType.EpicGames => new EpicStoreAuthenticationService(), 
			StoreType.GoG => new GogAuthenticationService(), 
			_ => new NotImplementedAuthenticationService(store), 
		};
	}

	public static IPlatformUser CreatePlatformUser(StoreType store)
	{
		return store switch
		{
			StoreType.Steam => new SteamPlatformUser(), 
			StoreType.EpicGames => new EpicGamesPlatformUser(), 
			StoreType.GoG => new GoGPlatformUser(), 
			_ => new DummyPlatformUser(), 
		};
	}
}
