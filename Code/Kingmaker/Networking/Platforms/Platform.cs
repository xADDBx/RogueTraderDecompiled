using System;
using System.Threading.Tasks;
using Kingmaker.Networking.Platforms.Authentication;
using Kingmaker.Networking.Platforms.Session;
using Kingmaker.Networking.Platforms.User;
using Kingmaker.Stores;

namespace Kingmaker.Networking.Platforms;

public abstract class Platform
{
	public StoreType StoreType { get; }

	public IPlatformUser User { get; }

	public IAuthenticationService AuthService { get; }

	public virtual IPlatformInvite Invite => PhotonManager.Invite.m_PlatformInvite;

	public IPlatformSession Session { get; }

	public virtual bool HasSecondaryPlatform => false;

	public virtual Platform SecondaryPlatform => null;

	public virtual Task InitSecondary()
	{
		throw new NotImplementedException();
	}

	protected Platform(StoreType store)
	{
		StoreType = store;
		User = PlatformServices.CreatePlatformUser(store);
		AuthService = PlatformServices.CreateAuthService(store);
		Session = PlatformServices.CreatePlatformSession(store);
	}

	public abstract bool IsInitialized();

	public abstract Task<bool> WaitInitialization();
}
