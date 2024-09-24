using System.Threading.Tasks;
using Kingmaker.Stores;
using Plugins.GOG;

namespace Kingmaker.Networking.Platforms;

public class GoGPlatform : Platform
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

	public GoGPlatform()
		: base(StoreType.GoG)
	{
	}

	public override bool IsInitialized()
	{
		return GogGalaxyManager.IsInitializedAndLoggedOn();
	}

	public override Task<bool> WaitInitialization()
	{
		return GogGalaxyManager.WaitInitialization();
	}

	public override async Task InitSecondary()
	{
		PFLog.Net.Log("[GoGPlatform.InitSecondary] ");
	}
}
