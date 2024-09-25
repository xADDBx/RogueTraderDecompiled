using System;
using System.Threading.Tasks;
using Kingmaker.Networking.Player;
using UnityEngine;

namespace Kingmaker.Networking.Platforms.User;

public class DummyPlatformUser : IPlatformUser
{
	private Texture2D m_Icon;

	string IPlatformUser.NickName => PhotonManager.Instance.LocalPlayerUserId;

	PlayerAvatar IPlatformUser.LargeIcon => PlayerAvatar.Invalid;

	public Task Initialize()
	{
		return Task.CompletedTask;
	}

	public void GetLargeIcon(string userId, Action<PlayerAvatar> callback)
	{
		Delay(userId, callback);
		static async Task Delay(string userId, Action<PlayerAvatar> callback)
		{
			PFLog.Net.Log("DummyPlatformUser.GetLargeIcon " + userId + " start");
			await Task.Delay(TimeSpan.FromSeconds(1.0));
			PFLog.Net.Log("DummyPlatformUser.GetLargeIcon " + userId + " finish");
			callback(PlayerAvatar.Invalid);
		}
	}
}
