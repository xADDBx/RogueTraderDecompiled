using System.Threading;
using System.Threading.Tasks;
using Photon.Realtime;
using UnityEngine;

namespace Kingmaker.Networking.Platforms.Authentication;

public class EditorAuthenticationService : IAuthenticationService
{
	public Task<AuthenticationValues> GetAuthData(CancellationToken cancellationToken)
	{
		return Task.FromResult(new AuthenticationValues(Application.dataPath + "_" + SystemInfo.deviceUniqueIdentifier.Substring(0, 6)));
	}

	public void OnConnected()
	{
	}
}
