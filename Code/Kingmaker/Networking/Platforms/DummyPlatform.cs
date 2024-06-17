using System.Threading.Tasks;
using Kingmaker.Stores;

namespace Kingmaker.Networking.Platforms;

public class DummyPlatform : Platform
{
	public DummyPlatform()
		: base(StoreType.None)
	{
	}

	public override bool IsInitialized()
	{
		return true;
	}

	public override Task<bool> WaitInitialization()
	{
		return Task.FromResult(result: true);
	}
}
