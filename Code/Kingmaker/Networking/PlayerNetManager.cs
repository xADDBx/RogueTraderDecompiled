using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Networking.Player;
using UnityEngine;

namespace Kingmaker.Networking;

public class PlayerNetManager
{
	private readonly Dictionary<string, Texture2D> m_IconLarge = new Dictionary<string, Texture2D>();

	public bool GetNickName(NetPlayer player, out string nickName)
	{
		if (PhotonManager.Instance.PlayerToPhotonPlayer(player, out var photonPlayer))
		{
			nickName = photonPlayer.NickName;
			return true;
		}
		nickName = null;
		return false;
	}

	public bool GetNickName(PhotonActorNumber player, out string nickName)
	{
		if (PhotonManager.Instance.ActorNumberToPhotonPlayer(player, out var player2))
		{
			nickName = player2.NickName;
			return true;
		}
		nickName = null;
		return false;
	}

	public void SetIconLarge(string userId, PlayerAvatar playerAvatar)
	{
		if (!playerAvatar.IsValid)
		{
			PFLog.Net.Error("PlayerAvatar is Invalid! Skipping for UserId='" + userId + "'");
		}
		else if (m_IconLarge.ContainsKey(userId))
		{
			PFLog.Net.Log("IconLarge already exists! Skipping for UserId='" + userId + "'");
		}
		else
		{
			m_IconLarge[userId] = CreateTexture(playerAvatar);
		}
	}

	public void SetIconLarge(string userId, Texture2D avatar)
	{
		m_IconLarge[userId] = avatar;
	}

	public bool GetIconLarge(PhotonActorNumber player, out Texture2D value)
	{
		if (!player.IsValid)
		{
			value = null;
			return false;
		}
		if (PhotonManager.Instance == null)
		{
			PFLog.Net.Warning("[GetIconLarge] PhotonManager.Instance is null");
			value = null;
			return false;
		}
		if (!PhotonManager.Instance.ActorNumberToPhotonPlayer(player, out var player2))
		{
			PFLog.Net.Warning("[GetIconLarge] Player=" + player.ToString() + " not found");
			value = null;
			return false;
		}
		return GetIconLarge(player2.UserId, out value);
	}

	public bool GetIconLarge(string userId, out Texture2D value)
	{
		bool num = m_IconLarge.TryGetValue(userId, out value);
		if (!num)
		{
			PFLog.Net.Warning("[GetIconLarge] UserId='" + userId + "' not found");
		}
		return num;
	}

	public void ClearCache([CanBeNull] string excludeUserId)
	{
		Texture2D texture2D = null;
		foreach (KeyValuePair<string, Texture2D> item in m_IconLarge)
		{
			if (item.Key == excludeUserId)
			{
				texture2D = item.Value;
			}
			else
			{
				Object.Destroy(item.Value);
			}
		}
		m_IconLarge.Clear();
		if (texture2D != null && excludeUserId != null)
		{
			m_IconLarge[excludeUserId] = texture2D;
		}
	}

	private static Texture2D CreateTexture(PlayerAvatar playerAvatar)
	{
		int width = playerAvatar.Width;
		int height = playerAvatar.Data.Length / width / 4;
		Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false, linear: true);
		texture2D.LoadRawTextureData(playerAvatar.Data);
		texture2D.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		return texture2D;
	}
}
