using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Networking.Save;
using Kingmaker.Networking.Settings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using UnityEngine;

namespace Kingmaker.Networking;

public class SaveNetManager
{
	public class SaveReceiveData
	{
		public readonly SaveInfoKey SaveInfoKey;

		public readonly uint RandomNoise;

		public SaveReceiveData(SaveInfoKey saveInfoKey, uint randomNoise)
		{
			SaveInfoKey = saveInfoKey;
			RandomNoise = randomNoise;
		}
	}

	private readonly struct UploadTextureData
	{
		public readonly byte[] TextureData;

		public readonly string SaveId;

		public UploadTextureData(SaveInfo saveInfo, string saveId)
		{
			using (saveInfo.GetReadScope())
			{
				TextureData = saveInfo.Saver.ReadBytes("header.png");
				SaveId = saveId;
			}
		}
	}

	private bool m_InProcess;

	private NetPlayerGroup m_Players = NetPlayerGroup.Empty;

	private Texture2D m_DownloadedSaveTexture;

	private UploadTextureData m_UploadTextureData;

	private int m_CurrentOffset;

	private int m_SaveBytesCount;

	private TaskCompletionSource<bool> m_DownloadSaveTcs;

	private PhotonActorNumber m_SaveFromPlayer;

	private readonly List<byte> m_DownloadSaveBytes = new List<byte>();

	private SaveMetaData m_DownloadSaveMetaData;

	private TaskCompletionSource<bool> m_UploadSaveAllPlayersReadyTcs;

	private Texture2D DownloadedSaveTexture
	{
		get
		{
			return m_DownloadedSaveTexture;
		}
		set
		{
			if (m_DownloadedSaveTexture != null)
			{
				UnityEngine.Object.Destroy(m_DownloadedSaveTexture);
			}
			m_DownloadedSaveTexture = value;
		}
	}

	public bool InProcess
	{
		get
		{
			return m_InProcess;
		}
		private set
		{
			if (m_InProcess != value)
			{
				m_InProcess = value;
				EventBus.RaiseEvent(delegate(INetEvents h)
				{
					h.HandleTransferProgressChanged(value);
				});
			}
		}
	}

	private static string GetSaveFilePath(string saveName)
	{
		return ApplicationPaths.persistentDataPath + "/Saved Games/" + saveName + ".zks";
	}

	public void ClearState()
	{
		PFLog.Net.Log("[SaveNetManager.ClearState]");
		InProcess = false;
		m_Players = NetPlayerGroup.Empty;
		m_CurrentOffset = 0;
		m_SaveBytesCount = 0;
		m_SaveFromPlayer = PhotonActorNumber.Invalid;
		m_DownloadSaveBytes.Clear();
		m_DownloadSaveMetaData = null;
		DownloadedSaveTexture = null;
		m_UploadTextureData = default(UploadTextureData);
	}

	public void OnPlayerEnteredRoom(PhotonActorNumber player)
	{
		if (PhotonManager.Instance.IsRoomOwner && m_UploadTextureData.SaveId != null)
		{
			PhotonManager.Instance.DataTransporter.SendSaveScreenshot(m_UploadTextureData.SaveId, m_UploadTextureData.TextureData, player);
		}
	}

	public void OnPlayerLeftRoom(PhotonActorNumber player)
	{
		if (m_SaveFromPlayer == player)
		{
			m_DownloadSaveTcs.TrySetException(new SaveSourceDisconnectedException());
		}
		CheckAllPlayersReady();
	}

	public bool GetSentProgress(out int progress, out int target)
	{
		if (InProcess)
		{
			progress = Mathf.Max(m_CurrentOffset, m_DownloadSaveBytes.Count);
			target = m_SaveBytesCount;
			return 0 < target;
		}
		progress = 0;
		target = 0;
		return false;
	}

	public void SelectSave([CanBeNull] SaveInfo saveInfo)
	{
		if (saveInfo != null)
		{
			SaveInfoShort data = (SaveInfoShort)saveInfo;
			PhotonManager.Instance.SetRoomProperty("si", data);
			m_UploadTextureData = new UploadTextureData(saveInfo, saveInfo.SaveId);
			PhotonManager.Instance.DataTransporter.SendSaveScreenshot(m_UploadTextureData.SaveId, m_UploadTextureData.TextureData);
		}
		else
		{
			PhotonManager.Instance.ClearRoomProperty("si");
			m_UploadTextureData = default(UploadTextureData);
		}
	}

	public void OnSelectedSaveUpdated()
	{
		if (PhotonManager.Instance.IsRoomOwner)
		{
			return;
		}
		PFLog.Net.Log("Applying selected SaveInfo...");
		if (PhotonManager.Instance.GetRoomProperty<SaveInfoShort>("si", out var obj) && !obj.IsEmpty)
		{
			SaveInfo saveInfo = (SaveInfo)obj;
			if (DownloadedSaveTexture != null)
			{
				saveInfo.Screenshot = DownloadedSaveTexture;
			}
			EventBus.RaiseEvent(delegate(INetSaveSelectHandler h)
			{
				h.HandleSaveSelect(saveInfo);
			});
			PFLog.Net.Log("SaveInfo was applied!");
		}
		else
		{
			DownloadedSaveTexture = null;
			EventBus.RaiseEvent(delegate(INetSaveSelectHandler h)
			{
				h.HandleSaveSelect(null);
			});
			PFLog.Net.Log("Room SaveInfo was not found!");
		}
	}

	public void UpdateSaveTexture(Texture2D saveTexture)
	{
		DownloadedSaveTexture = saveTexture;
		OnSelectedSaveUpdated();
	}

	public async Task UploadSave(SaveInfo saveInfo, uint randomNoise, CancellationToken cancellationToken)
	{
		if (InProcess)
		{
			throw new AlreadyInProgressException();
		}
		InProcess = true;
		if (!IsSuitableSaveType(saveInfo))
		{
			throw new SaveNotFoundException("Not supported save type");
		}
		m_CurrentOffset = 0;
		SaveInfoKey saveInfoKey = (SaveInfoKey)saveInfo;
		while (!PhotonManager.DLC.IsDLCsInLobbyReady)
		{
			await Task.Delay(16, cancellationToken);
		}
		PhotonActorNumber[] actorNumbersAtStart = PhotonManager.Instance.GetActorNumbersAtStart();
		string[] dlcs = PhotonManager.DLC.CreateDlcInGameList();
		PhotonManager.Instance.CloseRoom(actorNumbersAtStart, dlcs);
		PFLog.Net.Log("Send LoadSave to all players...");
		ArraySegment<byte> bytes;
		using (InitAllPlayersReadyTcs(cancellationToken))
		{
			if (!PhotonManager.Instance.SendMessageToOthers(1))
			{
				throw new SendMessageFailException("Failed to send LoadSave");
			}
			string folderName = saveInfo.FolderName;
			PFLog.Net.Log("UploadSave started... Save: " + saveInfoKey.ToString() + " Path: '" + folderName + "'");
			bytes = await Task.Run(() => SavePacker.RepackSaveToSend(saveInfo), cancellationToken);
			m_SaveBytesCount = bytes.Count;
			await m_UploadSaveAllPlayersReadyTcs.Task;
		}
		DateTime startTime = DateTime.Now;
		PFLog.Net.Log("Collecting settings...");
		BaseSettingNetData[] settings = PhotonManager.Settings.CollectState();
		PFLog.Net.Log("Creating and sending save meta data...");
		ByteArraySlice byteArraySlice = NetMessageSerializer.SerializeToSlice(new SaveMetaData
		{
			length = bytes.Count,
			saveName = saveInfoKey.Name,
			saveId = saveInfoKey.Id,
			randomNoise = randomNoise,
			actorNumbersAtStart = actorNumbersAtStart,
			dlcs = dlcs,
			settings = settings
		});
		using (InitAllPlayersReadyTcs(cancellationToken))
		{
			if (!PhotonManager.Instance.SendMessageToOthers(3, byteArraySlice.Buffer, byteArraySlice.Offset, byteArraySlice.Count))
			{
				throw new SendMessageFailException("Failed to send SaveMeta package");
			}
			byteArraySlice.Release();
			PFLog.Net.Log("SaveMeta message was send");
			await m_UploadSaveAllPlayersReadyTcs.Task;
		}
		int offset = 0;
		int index = 0;
		while (offset < bytes.Count)
		{
			int num = Mathf.Min(bytes.Count - offset, SaveMetaData.MaxPacketSize);
			ArraySegment<byte> arraySegment = bytes.Slice(offset, num);
			using (InitAllPlayersReadyTcs(cancellationToken))
			{
				if (!PhotonManager.Instance.SendMessageToOthers(4, arraySegment.Array, arraySegment.Offset, arraySegment.Count))
				{
					throw new SendMessageFailException($"Failed to send Save (package #{index + 1})");
				}
				offset += num;
				PFLog.Net.Log($"PacketType.Save message was send. Bytes: {offset}/{bytes.Count}");
				m_CurrentOffset = offset;
				await m_UploadSaveAllPlayersReadyTcs.Task;
			}
			int num2 = index + 1;
			index = num2;
		}
		PFLog.Net.Log($"[{Time.frameCount}] Uploading process complete! size={(double)m_SaveBytesCount / 1024.0:0.00}KB time={(DateTime.Now - startTime).TotalSeconds:0.00}s chunk={(double)SaveMetaData.MaxPacketSize / 1024.0:0.00}KB speed={(double)m_SaveBytesCount / (DateTime.Now - startTime).TotalSeconds / 1024.0 / 1024.0:0.00}MB/s");
		InProcess = false;
		m_CurrentOffset = 0;
		m_SaveBytesCount = 0;
		IDisposable InitAllPlayersReadyTcs(CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			m_UploadSaveAllPlayersReadyTcs = new TaskCompletionSource<bool>();
			m_Players = new NetPlayerGroup(PhotonManager.Instance.LocalNetPlayer);
			CheckAllPlayersReady();
			return token.CanBeCanceled ? token.Register(delegate
			{
				m_UploadSaveAllPlayersReadyTcs.TrySetCanceled();
			}) : default(CancellationTokenRegistration);
		}
	}

	public void OnRequestSave(NetPlayer player)
	{
		if (m_Players.Contains(player))
		{
			PFLog.Net.Error("OnRequestSave: message duplicated! Player #" + player);
			return;
		}
		m_Players = m_Players.Add(player);
		PFLog.Net.Log("OnRequestSave: Player #" + player);
		CheckAllPlayersReady();
	}

	public void OnSaveAcknowledge(NetPlayer player, ReadOnlySpan<byte> bytes)
	{
		if (bytes.Length != 4)
		{
			PFLog.Net.Error($"OnSaveAcknowledge: unexpected packet size! {bytes.Length}/{4}");
			return;
		}
		int num = BinaryPrimitives.ReadInt32BigEndian(bytes);
		if (m_CurrentOffset != num)
		{
			PFLog.Net.Error($"OnSaveAcknowledge: unexpected offset! Player #{player.ToString()} offset={num}/{m_CurrentOffset}");
			return;
		}
		if (m_Players.Contains(player))
		{
			PFLog.Net.Error("OnSaveAcknowledge: message duplicated! Player #" + player);
			return;
		}
		m_Players = m_Players.Add(player);
		PFLog.Net.Log("OnSaveAcknowledge: Player #" + player);
		CheckAllPlayersReady();
	}

	private void CheckAllPlayersReady()
	{
		if (m_Players.Contains(PhotonManager.Instance.PlayersReadyMask))
		{
			m_UploadSaveAllPlayersReadyTcs.TrySetResult(result: true);
		}
	}

	public async Task<SaveReceiveData> DownloadSave(PhotonActorNumber saveFromPlayer, CancellationToken cancellationToken)
	{
		if (InProcess)
		{
			throw new AlreadyInProgressException();
		}
		try
		{
			InProcess = true;
			m_DownloadSaveTcs = new TaskCompletionSource<bool>();
			using (cancellationToken.Register(delegate
			{
				m_DownloadSaveTcs.TrySetCanceled();
			}))
			{
				m_SaveFromPlayer = saveFromPlayer;
				PFLog.Net.Log("DownloadSave started...");
				if (!PhotonManager.Instance.SendMessageTo(saveFromPlayer, 2))
				{
					throw new SendMessageFailException("Failed to send RequestSave");
				}
				await m_DownloadSaveTcs.Task;
				PFLog.Net.Log("Downloading process completed!");
				SaveInfo saveInfo = Game.Instance.SaveManager.FirstOrDefault(SaveManager.IsCoopSave);
				if (saveInfo != null)
				{
					Game.Instance.SaveManager.DeleteSave(saveInfo);
				}
				await Task.Run(delegate
				{
					RepackAndSaveToFile(m_DownloadSaveBytes, GetSaveFilePath("net_save_4b31b8ad92353a02"));
				}, cancellationToken);
				SaveInfoKey saveInfoKey = new SaveInfoKey(m_DownloadSaveMetaData.saveId, "net_save_4b31b8ad92353a02", saveFromPlayer.ToNetPlayer(NetPlayer.Empty));
				PFLog.Net.Log($"SaveInfo {saveInfoKey}");
				return new SaveReceiveData(saveInfoKey, m_DownloadSaveMetaData.randomNoise);
			}
		}
		finally
		{
			InProcess = false;
			m_SaveFromPlayer = PhotonActorNumber.Invalid;
			m_DownloadSaveMetaData = null;
			m_DownloadSaveBytes.Clear();
		}
	}

	public void OnSaveMetaReceived(PhotonActorNumber player, ReadOnlySpan<byte> bytes)
	{
		try
		{
			m_DownloadSaveMetaData = NetMessageSerializer.DeserializeFromSpan<SaveMetaData>(bytes);
		}
		catch (Exception innerException)
		{
			m_DownloadSaveMetaData = null;
			m_DownloadSaveTcs.SetException(new SaveReceiveException("Can't parse SavePart", innerException));
			return;
		}
		m_DownloadSaveBytes.Clear();
		m_DownloadSaveBytes.IncreaseCapacity(m_DownloadSaveMetaData.length);
		m_SaveBytesCount = m_DownloadSaveMetaData.length;
		PhotonManager.Instance.CloseRoom(m_DownloadSaveMetaData.actorNumbersAtStart, m_DownloadSaveMetaData.dlcs);
		PFLog.Net.Log("Applying settings...");
		BaseSettingNetData[] settings = m_DownloadSaveMetaData.settings;
		for (int i = 0; i < settings.Length; i++)
		{
			settings[i].ForceSet();
		}
		PFLog.Net.Log("All settings was applied!");
		if (!SendSaveAcknowledge(player, m_DownloadSaveBytes.Count))
		{
			m_DownloadSaveTcs.SetException(new SendMessageFailException("Failed to send SaveAcknowledge"));
		}
	}

	public void OnSaveReceived(PhotonActorNumber player, ReadOnlySpan<byte> bytes)
	{
		ReadOnlySpan<byte> readOnlySpan = bytes;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			byte item = readOnlySpan[i];
			m_DownloadSaveBytes.Add(item);
		}
		PFLog.Net.Log($"Save bytes {m_DownloadSaveBytes.Count}/{m_DownloadSaveMetaData.length} received");
		int count = m_DownloadSaveBytes.Count;
		if (m_DownloadSaveBytes.Count == m_DownloadSaveMetaData.length)
		{
			m_DownloadSaveTcs.SetResult(result: true);
		}
		if (!SendSaveAcknowledge(player, count))
		{
			m_DownloadSaveTcs.SetException(new SendMessageFailException("Failed to send SaveAcknowledge"));
		}
	}

	public static bool IsSuitableSaveType(SaveInfo saveInfo)
	{
		if (File.Exists(saveInfo.FolderName))
		{
			if (!saveInfo.FileName.EndsWith(".zks"))
			{
				if (BuildModeUtility.IsDevelopment)
				{
					return saveInfo.FileName.EndsWith(".zip");
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private static bool SendSaveAcknowledge(PhotonActorNumber player, int currentLength)
	{
		byte[] array = MessageNetManager.SendBytes.GetArray();
		BinaryPrimitives.WriteInt32BigEndian(array.AsSpan(0, 4), currentLength);
		return PhotonManager.Instance.SendMessageTo(player, 5, array, 0, 4);
	}

	private static void RepackAndSaveToFile(List<byte> data, string filePath)
	{
		PFLog.Net.Log("Repacking data...");
		byte[] saveBytes = data.ToArray();
		saveBytes = SavePacker.RepackReceivedSave(saveBytes);
		PFLog.Net.Log("Saving to file... " + filePath);
		string directoryName = Path.GetDirectoryName(filePath);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		File.WriteAllBytes(filePath, saveBytes);
	}
}
