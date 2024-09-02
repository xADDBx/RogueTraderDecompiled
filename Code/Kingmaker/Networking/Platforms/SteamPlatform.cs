using System.Threading.Tasks;
using Kingmaker.Stores;

namespace Kingmaker.Networking.Platforms;

public class SteamPlatform : Platform
{
	private Platform m_SecondaryPlatform;

	public override bool HasSecondaryPlatform => false;

	public override Platform SecondaryPlatform
	{
		get
		{
			_ = HasSecondaryPlatform;
			return null;
		}
	}

	public SteamPlatform()
		: base(StoreType.Steam)
	{
	}

	public override bool IsInitialized()
	{
		return SteamManager.Initialized;
	}

	public override Task<bool> WaitInitialization()
	{
		return Task.FromResult(SteamManager.Initialized);
	}

	public override async Task InitSecondary()
	{
		PFLog.Net.Log("[SteamPlatform.InitSecondary] ");
	}
}
