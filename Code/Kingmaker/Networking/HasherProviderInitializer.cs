using Kingmaker.StateHasher;
using StateHasher.Core;

namespace Kingmaker.Networking;

public static class HasherProviderInitializer
{
	public static void InitializeDefault()
	{
		HasherProvider.Initialize(new PFHasherLogger());
	}
}
