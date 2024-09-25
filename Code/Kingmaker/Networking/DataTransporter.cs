using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Enums;
using Kingmaker.Networking.Player;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Stores;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using Photon.Realtime;
using UnityEngine;

namespace Kingmaker.Networking;

public class DataTransporter
{
	private readonly struct SenderInfo
	{
		public readonly DataSender Sender;

		[CanBeNull]
		public readonly TaskCompletionSource<bool> TaskCompletionSource;

		public SenderInfo(DataSender sender, TaskCompletionSource<bool> taskCompletionSource = null)
		{
			Sender = sender;
			TaskCompletionSource = taskCompletionSource;
		}
	}

	private readonly PhotonManager m_PhotonManager;

	private readonly CustomPortraitsManager m_PortraitsManager;

	private readonly List<SenderInfo> m_Senders = new List<SenderInfo>();

	private readonly List<DataReceiver> m_Receivers = new List<DataReceiver>();

	private CancellationTokenSource m_SendersFullStopCts;

	private string m_ScreenshotSaveId;

	private CancellationTokenSource m_ScreenshotCts;

	private int m_SenderUniqueNumber;

	public DataTransporterReceiversFactory ReceiversFactory { get; } = new DataTransporterReceiversFactory();


	public static bool CheatAlwaysSendAvatarViaPhoton { get; set; }

	public bool IsBusy
	{
		get
		{
			if (m_Senders.Count <= 0)
			{
				return m_Receivers.Count > 0;
			}
			return true;
		}
	}

	public event Action<DataTransferProgressInfo> CustomPortraitProgress;

	public event Action<DataReceiver> CreateNewReceiver;

	public DataTransporter(PhotonManager photonManager, CustomPortraitsManager portraitsManager)
	{
		m_PhotonManager = photonManager;
		m_PortraitsManager = portraitsManager;
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
		ScreenshotSender sender = new ScreenshotSender(targetActors, saveScreenshotData, ++m_SenderUniqueNumber, m_PhotonManager, m_ScreenshotCts.Token, m_SendersFullStopCts.Token);
		m_Senders.Add(new SenderInfo(sender));
		ProcessSenders();
	}

	public async Task SendCustomPortrait(PortraitData portrait, List<PhotonActorNumber> targetActors, IProgress<DataTransferProgressInfo> progress, CancellationToken interruptSendingCancellationToken = default(CancellationToken), CancellationToken fullStopSendingCancellationToken = default(CancellationToken))
	{
		m_SenderUniqueNumber++;
		PFLog.Net.Log($"[DataTransporter.SendCustomPortrait] id={portrait.CustomId}, N={m_SenderUniqueNumber}");
		await using (fullStopSendingCancellationToken.Register(m_SendersFullStopCts.Cancel))
		{
			CustomPortraitSender sender = new CustomPortraitSender(targetActors, portrait, m_SenderUniqueNumber, m_PhotonManager, interruptSendingCancellationToken, m_SendersFullStopCts.Token, progress);
			SenderInfo item = new SenderInfo(sender, new TaskCompletionSource<bool>());
			m_Senders.Add(item);
			ProcessSenders();
			await item.TaskCompletionSource.Task;
		}
	}

	public async Task SendSave(List<PhotonActorNumber> targetActors, ArraySegment<byte> saveBytes, CancellationToken cancellationToken, IProgress<DataTransferProgressInfo> progress)
	{
		PFLog.Net.Log("[DataTransporter.SendSave]");
		await using (cancellationToken.Register(m_SendersFullStopCts.Cancel))
		{
			SaveSender sender = new SaveSender(targetActors, saveBytes, ++m_SenderUniqueNumber, m_PhotonManager, default(CancellationToken), m_SendersFullStopCts.Token, progress);
			SenderInfo item = new SenderInfo(sender, new TaskCompletionSource<bool>());
			m_Senders.Add(item);
			ProcessSenders();
			await item.TaskCompletionSource.Task;
		}
	}

	public void OnMessage(byte code, PhotonActorNumber player, ReadOnlySpan<byte> bytes)
	{
		switch (code)
		{
		case 21:
		case 22:
		case 23:
		case 24:
		{
			PFLog.Net.Log($"[DataTransporter.OnMessage] create new receiver ({code}) from {player}");
			DataReceiver dataReceiver = CreateReceiver(code);
			dataReceiver.OnMetaReceived(player, bytes);
			m_Receivers.Add(dataReceiver);
			ProcessReceivers();
			break;
		}
		case 31:
			m_Receivers[0].OnDataReceived(player, bytes);
			break;
		case 30:
			if (m_Senders.Count > 0)
			{
				m_Senders[0].Sender.OnAck(player, bytes);
			}
			else
			{
				PFLog.Net.Error("[DataTransporter.OnMessage] empty senders list");
			}
			break;
		case 32:
			m_Receivers[0].OnCancel();
			break;
		default:
			PFLog.Net.Error($"[DataTransporter.OnReceived] not handled msg = {code}");
			break;
		}
		DataReceiver CreateReceiver(byte receiverType)
		{
			DataTransporterReceiversFactory.Args args = new DataTransporterReceiversFactory.Args(player, m_PhotonManager);
			if (!ReceiversFactory.TryCreate(receiverType, args, out var receiver))
			{
				switch (receiverType)
				{
				case 21:
					receiver = new AvatarReceiver(player, m_PhotonManager, OnAvatarReceived);
					break;
				case 22:
					receiver = new ScreenshotReceiver(player, m_PhotonManager, OnScreenshotReceived);
					break;
				case 23:
				{
					Progress<DataTransferProgressInfo> progress = new Progress<DataTransferProgressInfo>(OnCustomPortraitProgress);
					receiver = new CustomPortraitReceiver(player, m_PhotonManager, OnCustomPortraitReceived, OnCustomPortraitCancel, progress);
					break;
				}
				default:
					throw new ArgumentOutOfRangeException("receiverType", receiverType.ToString(), "Can't create receiver");
				}
			}
			this.CreateNewReceiver?.Invoke(receiver);
			return receiver;
		}
	}

	public void OnPlayerLeftRoom(PhotonActorNumber player)
	{
		foreach (SenderInfo sender in m_Senders)
		{
			sender.Sender.OnPlayerLeftRoom(player);
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
		AvatarSender sender = new AvatarSender(targetActors, data, ++m_SenderUniqueNumber, m_PhotonManager, CancellationToken.None, m_SendersFullStopCts.Token);
		m_Senders.Add(new SenderInfo(sender));
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

	private void OnCustomPortraitProgress(DataTransferProgressInfo info)
	{
		this.CustomPortraitProgress?.Invoke(info);
	}

	private void OnCustomPortraitReceived(CustomPortraitReceiver.Result result)
	{
		LogChannel net = PFLog.Net;
		object[] obj = new object[6] { result.PlayerSource, result.CustomPortraitId, result.PortraitGuid, null, null, null };
		ArraySegment<byte> smallPortraitBytes = result.SmallPortraitBytes;
		obj[3] = smallPortraitBytes.Count;
		smallPortraitBytes = result.HalfPortraitBytes;
		obj[4] = smallPortraitBytes.Count;
		smallPortraitBytes = result.FullPortraitBytes;
		obj[5] = smallPortraitBytes.Count;
		net.Log(string.Format("[DataTransporter.OnCustomPortraitReceived] {0}, {1}/{2}, lengths={3}/{4}/{5}", obj));
		PortraitData portraitData = m_PortraitsManager.CreateNew(fillDefaultPortraits: false);
		PFLog.Net.Log("[CustomPortraitsCollector.CheckComplete] create new portrait folder " + portraitData.CustomId);
		m_PortraitsManager.SetGuid(portraitData.CustomId, result.PortraitGuid);
		string newPortraitId = portraitData.CustomId;
		WriteToFile(PortraitType.SmallPortrait, result.SmallPortraitBytes);
		WriteToFile(PortraitType.HalfLengthPortrait, result.HalfPortraitBytes);
		WriteToFile(PortraitType.FullLengthPortrait, result.FullPortraitBytes);
		PFLog.Net.Log("[CustomPortraitsCollector.CheckComplete] completed, write to " + newPortraitId);
		void WriteToFile(PortraitType type, ArraySegment<byte> bytes)
		{
			string portraitPath = m_PortraitsManager.GetPortraitPath(newPortraitId, type);
			Directory.CreateDirectory(Path.GetDirectoryName(portraitPath));
			using FileStream output = File.Open(portraitPath, FileMode.Create);
			using BinaryWriter binaryWriter = new BinaryWriter(output);
			binaryWriter.Write(bytes.Array, bytes.Offset, bytes.Count);
		}
	}

	private void OnCustomPortraitCancel(Guid originPortraitGuid)
	{
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
			TaskCompletionSource<bool> tcs = m_Senders[0].TaskCompletionSource;
			try
			{
				await m_Senders[0].Sender.Send();
				tcs?.TrySetResult(result: true);
			}
			catch (OperationCanceledException)
			{
				tcs?.TrySetCanceled();
			}
			catch (Exception ex2)
			{
				PFLog.Net.Exception(ex2);
				tcs?.TrySetException(ex2);
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
			m_Receivers[0].Dispose();
			m_Receivers.RemoveAt(0);
		}
	}
}
