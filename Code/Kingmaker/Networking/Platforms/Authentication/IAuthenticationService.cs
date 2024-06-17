using System;
using System.Threading;
using System.Threading.Tasks;
using Photon.Realtime;

namespace Kingmaker.Networking.Platforms.Authentication;

public interface IAuthenticationService
{
	static readonly TimeSpan DefaultGetAuthDataTimeout;

	Task<AuthenticationValues> GetAuthData(CancellationToken cancellationToken);

	void OnConnected();

	async Task<AuthenticationValues> GetAuthData(CancellationToken cancellationToken, TimeSpan timeout)
	{
		using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, CancellationToken.None);
		cts.CancelAfter(timeout);
		try
		{
			return await GetAuthData(cts.Token);
		}
		catch (OperationCanceledException)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				throw;
			}
			throw new GetAuthDataTimeoutException();
		}
	}

	static IAuthenticationService()
	{
		DefaultGetAuthDataTimeout = TimeSpan.FromSeconds(10.0);
	}
}
