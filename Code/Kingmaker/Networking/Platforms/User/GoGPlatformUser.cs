using System;
using System.Threading.Tasks;
using Galaxy.Api;
using Kingmaker.Networking.Player;
using Plugins.GOG;

namespace Kingmaker.Networking.Platforms.User;

public class GoGPlatformUser : IPlatformUser
{
	private class UserInformationRetrieveException : Exception
	{
		public GalaxyID UserID { get; }

		public IUserInformationRetrieveListener.FailureReason Reason { get; }

		public UserInformationRetrieveException(GalaxyID userID, IUserInformationRetrieveListener.FailureReason reason)
		{
			UserID = userID;
			Reason = reason;
		}
	}

	private class UserInformationRetrieveListener : IUserInformationRetrieveListener
	{
		private readonly TaskCompletionSource<GalaxyID> _tcs;

		public UserInformationRetrieveListener(TaskCompletionSource<GalaxyID> tcs)
		{
			_tcs = tcs;
		}

		public override void OnUserInformationRetrieveSuccess(GalaxyID userID)
		{
			PFLog.Net.Log("Success retrieve");
			_tcs.TrySetResult(userID);
		}

		public override void OnUserInformationRetrieveFailure(GalaxyID userID, FailureReason failureReason)
		{
			PFLog.Net.Log("Fail retrieve");
			_tcs.TrySetException(new UserInformationRetrieveException(userID, failureReason));
		}
	}

	private const int LargeAvatarWidth = 184;

	private const int LargeAvatarSize = 135424;

	private readonly byte[] m_LargeAvatarBuffer = new byte[135424];

	public string NickName { get; private set; }

	public PlayerAvatar LargeIcon { get; private set; }

	public async Task Initialize()
	{
		if (!GogGalaxyManager.IsInitializedAndLoggedOn())
		{
			throw new StoreNotInitializedException();
		}
		TaskCompletionSource<GalaxyID> initTcs = new TaskCompletionSource<GalaxyID>();
		GalaxyID userID = GogGalaxyManager.Instance.UserId;
		GetLargeIcon(userID.ToString(), delegate(PlayerAvatar avatar)
		{
			LargeIcon = avatar;
			initTcs.TrySetResult(userID);
		});
		await initTcs.Task;
		NickName = GalaxyInstance.Friends().GetPersonaName();
	}

	public async void GetLargeIcon(string userId, Action<PlayerAvatar> callback)
	{
		if (!ulong.TryParse(userId, out var result))
		{
			return;
		}
		GalaxyID userID = new GalaxyID(result);
		if (!GalaxyInstance.Friends().IsFriendAvatarImageRGBAAvailable(userID, AvatarType.AVATAR_TYPE_LARGE))
		{
			TaskCompletionSource<GalaxyID> taskCompletionSource = new TaskCompletionSource<GalaxyID>();
			UserInformationRetrieveListener listener = new UserInformationRetrieveListener(taskCompletionSource);
			GalaxyInstance.Friends().RequestUserInformation(userID, 4u, listener);
			try
			{
				await taskCompletionSource.Task;
			}
			catch (UserInformationRetrieveException ex)
			{
				PFLog.Net.Log($"Request exception: {ex.UserID} {ex.Reason}");
				throw;
			}
			catch (Exception arg)
			{
				PFLog.Net.Log($"Unknown exception: {arg}");
				throw;
			}
		}
		GalaxyInstance.Friends().GetFriendAvatarImageRGBA(userID, AvatarType.AVATAR_TYPE_LARGE, m_LargeAvatarBuffer, 135424u);
		byte[] array = new byte[m_LargeAvatarBuffer.Length];
		m_LargeAvatarBuffer.CopyTo(array, 0);
		PlatformUserUtils.VerticalFlip(array, 184, 184);
		PlayerAvatar obj = new PlayerAvatar(184, array);
		callback?.Invoke(obj);
	}
}
