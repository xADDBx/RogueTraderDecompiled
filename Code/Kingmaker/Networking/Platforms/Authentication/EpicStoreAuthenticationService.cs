using System.Threading;
using System.Threading.Tasks;
using Kingmaker.EOSSDK;
using Photon.Realtime;

namespace Kingmaker.Networking.Platforms.Authentication;

internal class EpicStoreAuthenticationService : IAuthenticationService
{
	public Task<AuthenticationValues> GetAuthData(CancellationToken cancellationToken)
	{
		if (!EpicGamesManager.IsInitializedAndSignedIn())
		{
			throw new StoreNotInitializedException();
		}
		return Task.FromResult(CreateAuthenticationValues());
	}

	public void OnConnected()
	{
	}

	private static AuthenticationValues CreateAuthenticationValues()
	{
		AuthenticationValues authenticationValues = new AuthenticationValues();
		authenticationValues.AuthType = CustomAuthenticationType.Epic;
		authenticationValues.UserId = EpicGamesManager.LocalUser.UserId.ToString();
		authenticationValues.AddAuthParameter("token", EpicGamesManager.LocalUser.AuthToken.AccessToken.ToString());
		return authenticationValues;
	}
}
