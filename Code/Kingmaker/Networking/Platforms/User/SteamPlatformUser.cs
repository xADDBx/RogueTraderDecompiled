using System;
using System.Threading.Tasks;
using Kingmaker.Networking.Player;
using Steamworks;

namespace Kingmaker.Networking.Platforms.User;

public class SteamPlatformUser : IPlatformUser
{
	private readonly struct ImageInfo
	{
		public readonly int Descriptor;

		public readonly int Width;

		public readonly int Height;

		public ImageInfo(int descriptor, int width, int height)
		{
			Descriptor = descriptor;
			Width = width;
			Height = height;
		}
	}

	private PlayerAvatar? m_LargeIcon;

	public string NickName { get; private set; }

	public PlayerAvatar LargeIcon { get; private set; }

	public async Task Initialize()
	{
		if (!SteamManager.Initialized)
		{
			throw new StoreNotInitializedException();
		}
		NickName = SteamFriends.GetPersonaName();
		LargeIcon = await GetLargeIcon(SteamUser.GetSteamID());
	}

	public async void GetLargeIcon(string userId, Action<PlayerAvatar> callback)
	{
		if (ulong.TryParse(userId, out var result))
		{
			callback(await GetLargeIcon(new CSteamID(result)));
		}
	}

	private static async Task<PlayerAvatar> GetLargeIcon(CSteamID id)
	{
		int largeFriendAvatar = SteamFriends.GetLargeFriendAvatar(id);
		if (largeFriendAvatar == 0)
		{
			await RequestUserInformation(id);
			largeFriendAvatar = SteamFriends.GetLargeFriendAvatar(id);
		}
		ImageInfo info;
		switch (largeFriendAvatar)
		{
		case 0:
			PFLog.Net.Error("[SteamPlatformUser.GetLargeIcon] can't get icon");
			return PlayerAvatar.Invalid;
		case -1:
			info = await GetImageInfoAsync(id);
			break;
		default:
		{
			if (!SteamUtils.GetImageSize(largeFriendAvatar, out var pnWidth, out var pnHeight))
			{
				return PlayerAvatar.Invalid;
			}
			info = new ImageInfo(largeFriendAvatar, (int)pnWidth, (int)pnHeight);
			break;
		}
		}
		return GetCachedPlayerAvatar(info);
	}

	private static async Task RequestUserInformation(CSteamID id)
	{
		if (!SteamFriends.RequestUserInformation(id, bRequireNameOnly: false))
		{
			return;
		}
		TaskCompletionSource<bool> callbackTcs = new TaskCompletionSource<bool>();
		using (Callback<PersonaStateChange_t>.Create(delegate(PersonaStateChange_t response)
		{
			if (response.m_ulSteamID == id.m_SteamID && response.m_nChangeFlags.HasFlag(EPersonaChange.k_EPersonaChangeAvatar))
			{
				callbackTcs.SetResult(result: true);
			}
		}))
		{
			await callbackTcs.Task;
		}
	}

	private static async Task<ImageInfo> GetImageInfoAsync(CSteamID id)
	{
		TaskCompletionSource<ImageInfo> callbackTcs = new TaskCompletionSource<ImageInfo>();
		using (Callback<AvatarImageLoaded_t>.Create(delegate(AvatarImageLoaded_t response)
		{
			if (!(response.m_steamID != id))
			{
				callbackTcs.SetResult(new ImageInfo(response.m_iImage, response.m_iWide, response.m_iTall));
			}
		}))
		{
			return await callbackTcs.Task;
		}
	}

	private static PlayerAvatar GetCachedPlayerAvatar(ImageInfo info)
	{
		int num = info.Width * info.Height * 4;
		byte[] array = new byte[num];
		if (!SteamUtils.GetImageRGBA(info.Descriptor, array, num))
		{
			return PlayerAvatar.Invalid;
		}
		PlatformUserUtils.VerticalFlip(array, info.Width, info.Height);
		return new PlayerAvatar(info.Width, array);
	}
}
