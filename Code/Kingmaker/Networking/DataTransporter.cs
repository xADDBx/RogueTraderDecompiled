using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Kingmaker.Networking.Player;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Stores;
using Kingmaker.Utility.DotNetExtensions;
using Photon.Realtime;
using UnityEngine;

namespace Kingmaker.Networking;

public class DataTransporter
{
	private readonly PhotonManager m_PhotonManager;

	private readonly List<DataSender> m_Senders = new List<DataSender>();

	private readonly List<DataReceiver> m_Receivers = new List<DataReceiver>();

	private CancellationTokenSource m_SendersFullStopCts;

	private string m_ScreenshotSaveId;

	private CancellationTokenSource m_ScreenshotCts;

	private int m_SenderUniqueNumber;

	public static bool CheatAlwaysSendAvatarViaPhoton { get; set; }

	public DataTransporter(PhotonManager photonManager)
	{
		m_PhotonManager = photonManager;
	}

	public void SendSaveScreenshot(string saveId, byte[] saveScreenshotData)
	{
		ReadonlyList<PlayerInfo> allPlayers = m_PhotonManager.AllPlayers;
		if (allPlayers.Count <= 1)
		{
			return;
		}
		List<PhotonActorNumber> list = new List<PhotonActorNumber>(allPlayers.Count - 1);
		foreach (PlayerInfo item in allPlayers)
		{
			if (item.UserId != m_PhotonManager.LocalPlayerUserId)
			{
				list.Add(item.Player);
			}
		}
		SendSaveScreenshot(saveId, saveScreenshotData, list);
	}

	public void SendSaveScreenshot(string saveId, byte[] saveScreenshotData, PhotonActorNumber player)
	{
		SendSaveScreenshot(saveId, saveScreenshotData, new List<PhotonActorNumber> { player });
	}

	private void SendSaveScreenshot(string saveId, byte[] saveScreenshotData, List<PhotonActorNumber> targetActors)
	{
		PFLog.Net.Log("[DataTransporter.SendSaveScreenshot] to " + string.Join(", ", targetActors));
		if (m_ScreenshotSaveId != saveId)
		{
			m_ScreenshotSaveId = saveId;
			m_ScreenshotCts?.Cancel();
			m_ScreenshotCts = new CancellationTokenSource();
		}
		ScreenshotSender item = new ScreenshotSender(targetActors, saveScreenshotData, ++m_SenderUniqueNumber, m_PhotonManager, m_ScreenshotCts.Token, m_SendersFullStopCts.Token);
		m_Senders.Add(item);
		ProcessSenders();
	}

	public void OnMessage(byte code, PhotonActorNumber player, ReadOnlySpan<byte> bytes)
	{
		switch (code)
		{
		case 21:
		case 22:
		{
			PFLog.Net.Log($"[DataTransporter.OnMessage] create new receiver from {player}");
			DataReceiver dataReceiver = CreateReceiver(code);
			dataReceiver.OnMetaReceived(player, bytes);
			m_Receivers.Add(dataReceiver);
			ProcessReceivers();
			break;
		}
		case 24:
			m_Receivers[0].OnDataReceived(player, bytes);
			break;
		case 23:
			m_Senders[0].OnAck(player, bytes);
			break;
		case 25:
			m_Receivers[0].OnCancel();
			break;
		default:
			PFLog.Net.Error($"[DataTransporter.OnReceived] not handled msg = {code}");
			break;
		}
		DataReceiver CreateReceiver(byte receiverType)
		{
			return receiverType switch
			{
				21 => new AvatarReceiver(player, m_PhotonManager, OnAvatarReceived), 
				22 => new ScreenshotReceiver(player, m_PhotonManager, OnScreenshotReceived), 
				_ => throw new ArgumentOutOfRangeException("receiverType", receiverType.ToString(), "Can't create receiver"), 
			};
		}
	}

	public void OnPlayerLeftRoom(PhotonActorNumber player)
	{
		foreach (DataSender sender in m_Senders)
		{
			sender.OnPlayerLeftRoom(player);
		}
		foreach (DataReceiver receiver in m_Receivers)
		{
			receiver.OnPlayerLeftRoom(player);
		}
	}

	public void OnJoinedLobby(Dictionary<int, Photon.Realtime.Player> allPlayers)
	{
		m_SendersFullStopCts = new CancellationTokenSource();
		List<PhotonActorNumber> list = null;
		if (!m_PhotonManager.LocalPlatformUser.LargeIcon.IsValid)
		{
			return;
		}
		foreach (KeyValuePair<int, Photon.Realtime.Player> allPlayer in allPlayers)
		{
			Photon.Realtime.Player player = allPlayer.Value;
			if (player.IsLocal)
			{
				continue;
			}
			if (player.TryGetProperty<StoreType>("s", out var value))
			{
				if (StoreManager.Store == value && !CheatAlwaysSendAvatarViaPhoton)
				{
					m_PhotonManager.LocalPlatformUser.GetLargeIcon(player.UserId, delegate(PlayerAvatar playerAvatar)
					{
						PhotonManager.Player.SetIconLarge(player.UserId, playerAvatar);
						EventBus.RaiseEvent(delegate(INetLobbyPlayersHandler h)
						{
							h.HandlePlayerChanged();
						});
					});
				}
				else
				{
					if (list == null)
					{
						list = new List<PhotonActorNumber>();
					}
					list.Add(new PhotonActorNumber(player.ActorNumber));
				}
			}
			EventBus.RaiseEvent(delegate(INetLobbyPlayersHandler h)
			{
				h.HandlePlayerChanged();
			});
		}
		if (list != null)
		{
			SendAvatar(list, m_PhotonManager.LocalPlatformUser.LargeIcon);
		}
	}

	public void OnStoreUpdated(Photon.Realtime.Player targetPlayer, StoreType storeType)
	{
		if (targetPlayer.IsLocal || !m_PhotonManager.LocalPlatformUser.LargeIcon.IsValid)
		{
			return;
		}
		if (StoreManager.Store == storeType && !CheatAlwaysSendAvatarViaPhoton)
		{
			m_PhotonManager.LocalPlatformUser.GetLargeIcon(targetPlayer.UserId, delegate(PlayerAvatar playerAvatar)
			{
				PhotonManager.Player.SetIconLarge(targetPlayer.UserId, playerAvatar);
				EventBus.RaiseEvent(delegate(INetLobbyPlayersHandler h)
				{
					h.HandlePlayerChanged();
				});
			});
		}
		else
		{
			SendAvatar(new List<PhotonActorNumber>
			{
				new PhotonActorNumber(targetPlayer.ActorNumber)
			}, m_PhotonManager.LocalPlatformUser.LargeIcon);
		}
	}

	public void OnLeave()
	{
		m_SendersFullStopCts?.Cancel();
		foreach (DataReceiver receiver in m_Receivers)
		{
			receiver.OnCancel();
		}
	}

	private void SendAvatar([NotNull] List<PhotonActorNumber> targetActors, PlayerAvatar data)
	{
		PFLog.Net.Log("[DataTransporter.SendAvatar] to " + string.Join(", ", targetActors));
		AvatarSender item = new AvatarSender(targetActors, data, ++m_SenderUniqueNumber, m_PhotonManager, CancellationToken.None, m_SendersFullStopCts.Token);
		m_Senders.Add(item);
		ProcessSenders();
	}

	private void OnScreenshotReceived(PhotonActorNumber targetPlayer, ImageMetaData m, byte[] bytes)
	{
		Texture2D texture2D = new Texture2D(4, 4, TextureFormat.DXT1, mipChain: false)
		{
			name = "Save screenshot Coop"
		};
		texture2D.LoadImage(bytes);
		texture2D.Compress(highQuality: false);
		texture2D.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		PhotonManager.Save.UpdateSaveTexture(texture2D);
	}

	private void OnAvatarReceived(PhotonActorNumber targetPlayer, PlayerAvatar avatar)
	{
		PFLog.Net.Log($"[PhotonManager.OnAvatarReceived] {targetPlayer}");
		foreach (PlayerInfo allPlayer in m_PhotonManager.AllPlayers)
		{
			if (allPlayer.Player == targetPlayer)
			{
				PhotonManager.Player.SetIconLarge(allPlayer.UserId, avatar);
				EventBus.RaiseEvent(delegate(INetLobbyPlayersHandler h)
				{
					h.HandlePlayerChanged();
				});
				break;
			}
		}
	}

	private async void ProcessSenders()
	{
		if (m_Senders.Count > 1)
		{
			PFLog.Net.Log($"[DataTransporter.ProcessSenders] busy, queue size {m_Senders.Count}");
			return;
		}
		while (m_Senders.Count > 0)
		{
			try
			{
				await m_Senders[0].Send();
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception ex2)
			{
				PFLog.Net.Exception(ex2);
			}
			m_Senders.RemoveAt(0);
		}
	}

	private async void ProcessReceivers()
	{
		if (m_Receivers.Count > 1)
		{
			PFLog.Net.Log($"[DataTransporter.ProcessReceivers] busy, queue size {m_Receivers.Count}");
			return;
		}
		while (m_Receivers.Count > 0)
		{
			try
			{
				await m_Receivers[0].StartReceiving();
			}
			catch (OperationCanceledException)
			{
			}
			catch (Exception ex2)
			{
				PFLog.Net.Exception(ex2);
			}
			m_Receivers.RemoveAt(0);
		}
	}
}
