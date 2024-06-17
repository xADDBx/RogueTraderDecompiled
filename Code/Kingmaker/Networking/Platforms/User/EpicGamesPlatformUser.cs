using System;
using System.Threading.Tasks;
using Epic.OnlineServices.Data;
using Kingmaker.EOSSDK;
using Kingmaker.Networking.Player;

namespace Kingmaker.Networking.Platforms.User;

public class EpicGamesPlatformUser : IPlatformUser
{
	public string NickName { get; private set; }

	public PlayerAvatar LargeIcon => PlayerAvatar.Invalid;

	public async Task Initialize()
	{
		if (!EpicGamesManager.IsInitializedAndSignedIn())
		{
			throw new StoreNotInitializedException();
		}
		await RetrieveUserNameIfNeeded();
		NickName = EpicGamesManager.LocalUser.Name;
	}

	public void GetLargeIcon(string userId, Action<PlayerAvatar> callback)
	{
		callback?.Invoke(PlayerAvatar.Invalid);
	}

	private static async Task RetrieveUserNameIfNeeded()
	{
		Epic.OnlineServices.Data.User user = EpicGamesManager.LocalUser;
		if (user.UpdateState != UserUpdateState.Done)
		{
			TaskCompletionSource<bool> updateTcs = new TaskCompletionSource<bool>();
			EventHandler onUpdated = delegate
			{
				updateTcs.SetResult(result: true);
			};
			user.Updated += onUpdated;
			user.Update();
			await updateTcs.Task;
			user.Updated -= onUpdated;
		}
	}
}
