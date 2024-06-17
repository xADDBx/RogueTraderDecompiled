using System.Threading;
using System.Threading.Tasks;
using Kingmaker.Stores;
using Photon.Realtime;

namespace Kingmaker.Networking.Platforms.Authentication;

internal class NotImplementedAuthenticationService : IAuthenticationService
{
	public NotImplementedAuthenticationService(StoreType storeType)
	{
		PFLog.Net.Error($"[NotImplementedAuthenticationService] {storeType} isn't implemented or defines for store are disabled");
	}

	public Task<AuthenticationValues> GetAuthData(CancellationToken cancellationToken)
	{
		return Task.FromResult(new AuthenticationValues());
	}

	public void OnConnected()
	{
	}
}
