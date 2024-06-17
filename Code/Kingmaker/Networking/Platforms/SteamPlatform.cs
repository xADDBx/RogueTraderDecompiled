using System.Threading.Tasks;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Kingmaker.EOSSDK;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Stores;

namespace Kingmaker.Networking.Platforms;

public class SteamPlatform : Platform
{
	private Platform m_SecondaryPlatform;

	public override bool HasSecondaryPlatform => true;

	public override Platform SecondaryPlatform
	{
		get
		{
			if (!HasSecondaryPlatform)
			{
				return null;
			}
			if (m_SecondaryPlatform == null)
			{
				m_SecondaryPlatform = PlatformServices.CreatePlatform(StoreType.EpicGames, primaryPlatform: false);
			}
			return m_SecondaryPlatform;
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
		Credentials loginOptionsCredentials = default(Credentials);
		loginOptionsCredentials.Type = LoginCredentialType.ExternalAuth;
		loginOptionsCredentials.Token = SteamManager.Instance.GetSessionTicket();
		loginOptionsCredentials.ExternalType = ExternalCredentialType.SteamSessionTicket;
		EpicGamesManager.StartManager(loginOptionsCredentials);
		if (await EpicGamesManager.Instance.WaitForSignIn())
		{
			await SecondaryPlatform.User.Initialize();
			EventBus.RaiseEvent(delegate(INetLobbyEpicGamesEvents h)
			{
				h.HandleSetEpicGamesUserName(EpicGamesManager.IsInitializedAndSignedIn(), EpicGamesManager.LocalUser.Name);
			});
			SecondaryPlatform.Invite.StartAnnounceGame();
		}
	}
}
