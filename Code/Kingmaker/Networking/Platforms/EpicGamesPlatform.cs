using System.Threading.Tasks;
using Kingmaker.EOSSDK;
using Kingmaker.Stores;

namespace Kingmaker.Networking.Platforms;

public class EpicGamesPlatform : Platform
{
	private readonly bool m_PrimaryPlatform;

	private IPlatformInvite m_Invite;

	public override IPlatformInvite Invite
	{
		get
		{
			if (m_PrimaryPlatform)
			{
				return base.Invite;
			}
			return m_Invite ?? (m_Invite = PlatformInviteFactory.Create(StoreType.EpicGames));
		}
	}

	public EpicGamesPlatform(bool primaryPlatform)
		: base(StoreType.EpicGames)
	{
		m_PrimaryPlatform = primaryPlatform;
	}

	public override bool IsInitialized()
	{
		return EpicGamesManager.IsInitializedAndSignedIn();
	}

	public override Task<bool> WaitInitialization()
	{
		return EpicGamesManager.Instance.WaitForSignIn();
	}
}
