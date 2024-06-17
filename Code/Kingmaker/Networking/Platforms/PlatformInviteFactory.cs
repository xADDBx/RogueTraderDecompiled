using Kingmaker.Stores;

namespace Kingmaker.Networking.Platforms;

public static class PlatformInviteFactory
{
	public static IPlatformInvite Create()
	{
		return Create(StoreManager.Store);
	}

	public static IPlatformInvite Create(StoreType store)
	{
		return store switch
		{
			StoreType.Steam => new SteamPlatformInvite(), 
			StoreType.EpicGames => new EpicGamesStorePlatformInvite(), 
			StoreType.GoG => new GoGPlatformInvite(), 
			_ => new DummyPlatformInvite(), 
		};
	}
}
