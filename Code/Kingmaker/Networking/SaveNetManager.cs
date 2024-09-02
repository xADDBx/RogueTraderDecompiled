using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Enums;
using Kingmaker.Networking.Player;
using Kingmaker.Networking.Save;
using Kingmaker.Networking.Settings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using UnityEngine;

namespace Kingmaker.Networking;

public class SaveNetManager
{
	public class SaveReceiveData
	{
		public readonly string SaveFilePath;

		public readonly uint RandomNoise;

		public SaveReceiveData(string saveFilePath, uint randomNoise)
		{
			SaveFilePath = saveFilePath;
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

	private class SaveTransferProgress : IProgress<DataTransferProgressInfo>
	{
		public int SaveBytesCount;

		public int PortraitsBytesCount;

		public int TotalCurrentOffset { get; private set; }

		public int TotalBytesCount => SaveBytesCount + PortraitsBytesCount;

		public void Reset()
		{
			SaveBytesCount = 0;
			PortraitsBytesCount = 0;
			TotalCurrentOffset = 0;
		}

		public void Report(DataTransferProgressInfo info)
		{
			TotalCurrentOffset += info.ProgressChange;
		}
	}

	private bool m_InProcess;

	private StreamsController m_StreamsController;

	private Texture2D m_DownloadedSaveTexture;

	private UploadTextureData m_UploadTextureData;

	private readonly SaveTransferProgress m_Progress = new SaveTransferProgress();

	private TaskCompletionSource<byte[]> m_DownloadSaveTcs;

	private SaveMetaData m_DownloadSaveMetaData;

	private readonly Dictionary<string, Guid> m_OriginIdGuidPortraitsData = new Dictionary<string, Guid>();

	private readonly Dictionary<Guid, List<PhotonActorNumber>> m_CustomPortraitsReceivers = new Dictionary<Guid, List<PhotonActorNumber>>();

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

	public PhotonActorNumber SaveFromPlayer { get; private set; }

	private static string GetSaveFilePath(string saveName)
	{
		return ApplicationPaths.temporaryCachePath + "/" + saveName + ".zks";
	}

	public void ClearState()
	{
		PFLog.Net.Log("[SaveNetManager.ClearState]");
		InProcess = false;
		m_StreamsController?.Clear();
		m_StreamsController = null;
		m_Progress.Reset();
		SaveFromPlayer = PhotonActorNumber.Invalid;
		m_DownloadSaveMetaData = null;
		DownloadedSaveTexture = null;
		m_UploadTextureData = default(UploadTextureData);
		m_CustomPortraitsReceivers.Clear();
		m_OriginIdGuidPortraitsData.Clear();
		PhotonManager.Instance.DataTransporter.CustomPortraitProgress -= m_Progress.Report;
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
		if (SaveFromPlayer == player)
		{
			m_DownloadSaveTcs.TrySetException(new SaveSourceDisconnectedException());
		}
		m_StreamsController?.OnPlayerLeftRoom(player);
	}

	public bool GetSentProgress(out int progress, out int target)
	{
		if (InProcess)
		{
			progress = m_Progress.TotalCurrentOffset;
			target = m_Progress.TotalBytesCount;
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
			SaveInfoShort saveInfoShort = (SaveInfoShort)saveInfo;
			saveInfoShort = SavePacker.RepackSaveInfoShortToSend(saveInfoShort);
			PhotonManager.Instance.SetRoomProperty("si", saveInfoShort);
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
			obj = SavePacker.RepackReceivedSaveInfoShort(obj);
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
		try
		{
			InProcess = true;
			if (!IsSuitableSaveType(saveInfo))
			{
				throw new SaveNotFoundException("Not supported save type");
			}
			m_Progress.Reset();
			SaveInfoKey saveInfoKey = (SaveInfoKey)saveInfo;
			while (!PhotonManager.DLC.IsDLCsInLobbyReady)
			{
				await Task.Delay(16, cancellationToken);
			}
			PhotonActorNumber[] actorNumbersAtStart = PhotonManager.Instance.GetActorNumbersAtStart();
			string[] dlcs = PhotonManager.DLC.CreateDlcInGameList();
			PhotonManager.Instance.CloseRoom(actorNumbersAtStart, dlcs);
			m_StreamsController = new StreamsController(GetTargetActors(), 1);
			PFLog.Net.Log("Send LoadSave to all players...");
			ArraySegment<byte> bytes;
			using (m_StreamsController.InitAllPlayersReadyTcs(cancellationToken))
			{
				if (!PhotonManager.Instance.SendMessageToOthers(1))
				{
					throw new SendMessageFailException("Failed to send LoadSave");
				}
				string folderName = saveInfo.FolderName;
				PFLog.Net.Log("UploadSave started... Save: " + saveInfoKey.ToString() + " Path: '" + folderName + "'");
				bytes = await Task.Run(() => SavePacker.RepackSaveToSend(saveInfo), cancellationToken);
				m_Progress.SaveBytesCount = bytes.Count;
				await m_StreamsController.WaitAllStreams();
			}
			PortraitData[] customPortraits = (from p in saveInfo.PartyPortraits
				where p.Data.IsCustom && CustomPortraitsManager.Instance.HasPortraits(p.Data.CustomId)
				select p.Data).ToArray();
			PortraitSaveMetaData[] portraitSaveMetaData = CreatePortraitSaveMetaData(customPortraits);
			DateTime startTime = DateTime.Now;
			PFLog.Net.Log("Collecting settings...");
			BaseSettingNetData[] settings = PhotonManager.Settings.CollectState();
			await SendSaveMeta(randomNoise, cancellationToken);
			await SendPortraits(customPortraits, m_CustomPortraitsReceivers, portraitSaveMetaData, m_Progress, cancellationToken);
			await PhotonManager.Instance.DataTransporter.SendSave(GetTargetActors(), bytes, cancellationToken, m_Progress);
			PFLog.Net.Log($"[{Time.frameCount}] Uploading process complete! size={(double)m_Progress.SaveBytesCount / 1024.0:0.00}KB time={(DateTime.Now - startTime).TotalSeconds:0.00}s chunk={(double)SaveMetaData.MaxPacketSize / 1024.0:0.00}KB streams={StreamsController.DefaultStreamsCount} speed={(double)m_Progress.SaveBytesCount / (DateTime.Now - startTime).TotalSeconds / 1024.0 / 1024.0:0.00}MB/s");
			async Task SendSaveMeta(uint noise, CancellationToken token)
			{
				PFLog.Net.Log("Creating and sending save meta data...");
				ByteArraySlice byteArraySlice = NetMessageSerializer.SerializeToSlice(new SaveMetaData
				{
					length = bytes.Count,
					saveName = saveInfoKey.Name,
					saveId = saveInfoKey.Id,
					randomNoise = noise,
					actorNumbersAtStart = actorNumbersAtStart,
					dlcs = dlcs,
					settings = settings,
					portraitsSaveMeta = portraitSaveMetaData
				});
				using (m_StreamsController.InitAllPlayersReadyTcs(token))
				{
					if (!PhotonManager.Instance.SendMessageToOthers(3, byteArraySlice.Buffer, byteArraySlice.Offset, byteArraySlice.Count))
					{
						throw new SendMessageFailException("Failed to send SaveMeta package");
					}
					byteArraySlice.Release();
					PFLog.Net.Log("SaveMeta message was send");
					await m_StreamsController.WaitAllStreams();
				}
			}
		}
		finally
		{
			InProcess = false;
			m_Progress.Reset();
		}
		static PortraitSaveMetaData[] CreatePortraitSaveMetaData(PortraitData[] portraits)
		{
			if (portraits.Empty())
			{
				return Array.Empty<PortraitSaveMetaData>();
			}
			return portraits.Select((PortraitData portrait) => new PortraitSaveMetaData
			{
				guid = CustomPortraitsManager.Instance.GetOrCreatePortraitGuid(portrait.CustomId),
				originId = portrait.CustomId,
				imagesFileLength = new int[3]
				{
					(int)new FileInfo(CustomPortraitsManager.Instance.GetPortraitPath(portrait.CustomId, PortraitType.SmallPortrait)).Length,
					(int)new FileInfo(CustomPortraitsManager.Instance.GetPortraitPath(portrait.CustomId, PortraitType.HalfLengthPortrait)).Length,
					(int)new FileInfo(CustomPortraitsManager.Instance.GetPortraitPath(portrait.CustomId, PortraitType.FullLengthPortrait)).Length
				}
			}).ToArray();
		}
		static List<PhotonActorNumber> GetTargetActors()
		{
			List<PhotonActorNumber> list = new List<PhotonActorNumber>();
			PhotonActorNumber photonActorNumber = new PhotonActorNumber(PhotonManager.Instance.LocalClientId);
			foreach (PlayerInfo activePlayer in PhotonManager.Instance.ActivePlayers)
			{
				if (activePlayer.Player != photonActorNumber)
				{
					list.Add(activePlayer.Player);
				}
			}
			return list;
		}
		static async Task SendPortraits(PortraitData[] customPortraits, Dictionary<Guid, List<PhotonActorNumber>> receivers, PortraitSaveMetaData[] portraitsMetaData, SaveTransferProgress progress, CancellationToken cancellationToken)
		{
			if (customPortraits.Empty())
			{
				PFLog.Net.Log("[SaveNetManager.UploadSave] portraits list empty");
			}
			else
			{
				foreach (PortraitData portraitData in customPortraits)
				{
					Guid guid = CustomPortraitsManager.Instance.GetOrCreatePortraitGuid(portraitData.CustomId);
					if (receivers.TryGetValue(guid, out var value))
					{
						int[] array = (from g in portraitsMetaData
							where g.guid == guid
							select g.imagesFileLength).First();
						foreach (int num in array)
						{
							progress.PortraitsBytesCount += num;
						}
						await PhotonManager.Instance.DataTransporter.SendCustomPortrait(portraitData, value, progress, default(CancellationToken), cancellationToken);
					}
				}
			}
		}
	}

	public void OnRequestSave(PhotonActorNumber player)
	{
		m_StreamsController.OnAck(player, 0);
	}

	public void OnSaveMetaAcknowledge(NetPlayer player, PhotonActorNumber photonActorNumber, ReadOnlySpan<byte> bytes)
	{
		SaveMetaAcknowledgeData saveMetaAcknowledgeData;
		try
		{
			saveMetaAcknowledgeData = NetMessageSerializer.DeserializeFromSpan<SaveMetaAcknowledgeData>(bytes);
		}
		catch (Exception innerException)
		{
			SaveReceiveException exception = new SaveReceiveException("Can't parse SaveMetaAcknowledgeData", innerException);
			m_StreamsController.Failed(exception);
			return;
		}
		PFLog.Net.Log($"[SaveNetManager.OnSaveMetaAcknowledge] {player}, portraitsCount={saveMetaAcknowledgeData.PortraitsGuid.Length}");
		SaveMetaAcknowledgeData.GuidData[] portraitsGuid = saveMetaAcknowledgeData.PortraitsGuid;
		for (int i = 0; i < portraitsGuid.Length; i++)
		{
			SaveMetaAcknowledgeData.GuidData guidData = portraitsGuid[i];
			if (m_CustomPortraitsReceivers.TryGetValue(guidData.Guid, out var value))
			{
				value.Add(photonActorNumber);
				continue;
			}
			m_CustomPortraitsReceivers[guidData.Guid] = new List<PhotonActorNumber> { photonActorNumber };
		}
		PFLog.Net.Log("OnSaveMetaAcknowledge: Player #" + player);
		m_StreamsController.OnAck(photonActorNumber, 0);
	}

	public async Task<SaveReceiveData> DownloadSave(PhotonActorNumber saveFromPlayer, CancellationToken cancellationToken)
	{
		if (InProcess)
		{
			throw new AlreadyInProgressException();
		}
		DataTransporter transporter = PhotonManager.Instance.DataTransporter;
		try
		{
			InProcess = true;
			m_DownloadSaveTcs = new TaskCompletionSource<byte[]>();
			SaveReceiveData result;
			await using (cancellationToken.Register(delegate
			{
				m_DownloadSaveTcs.TrySetCanceled();
			}))
			{
				SaveFromPlayer = saveFromPlayer;
				transporter.ReceiversFactory.Register(24, (DataTransporterReceiversFactory.Args args) => new SaveReceiver(args.SourcePlayer, args.Sender, delegate(byte[] data)
				{
					m_DownloadSaveTcs.TrySetResult(data);
				}, m_Progress, cancellationToken));
				PFLog.Net.Log("FillAllPortraitsGuid");
				CustomPortraitsManager.Instance.FillAllPortraitsGuid();
				PFLog.Net.Log("DownloadSave started...");
				if (!PhotonManager.Instance.SendMessageTo(saveFromPlayer, 2))
				{
					throw new SendMessageFailException("Failed to send RequestSave");
				}
				byte[] saveBytes = await m_DownloadSaveTcs.Task;
				PFLog.Net.Log("Downloading process completed!");
				SaveInfo saveInfo = Game.Instance.SaveManager.FirstOrDefault(SaveManager.IsCoopSave);
				if (saveInfo != null)
				{
					Game.Instance.SaveManager.DeleteSave(saveInfo);
				}
				string saveFilePath = GetSaveFilePath("net_save_4b31b8ad92353a02");
				await Task.Run(delegate
				{
					RepackAndSaveToFile(saveBytes, saveFilePath);
				}, cancellationToken);
				PFLog.Net.Log("Save Id " + m_DownloadSaveMetaData.saveId);
				result = new SaveReceiveData(saveFilePath, m_DownloadSaveMetaData.randomNoise);
			}
			return result;
		}
		finally
		{
			InProcess = false;
			if (m_DownloadSaveMetaData != null && m_DownloadSaveMetaData.portraitsSaveMeta != null)
			{
				PortraitSaveMetaData[] portraitsSaveMeta = m_DownloadSaveMetaData.portraitsSaveMeta;
				foreach (PortraitSaveMetaData portraitSaveMetaData in portraitsSaveMeta)
				{
					m_OriginIdGuidPortraitsData[portraitSaveMetaData.originId] = portraitSaveMetaData.guid;
				}
			}
			SaveFromPlayer = PhotonActorNumber.Invalid;
			m_DownloadSaveMetaData = null;
			transporter.ReceiversFactory.Unregister(24);
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
		m_Progress.SaveBytesCount = m_DownloadSaveMetaData.length;
		PhotonManager.Instance.CloseRoom(m_DownloadSaveMetaData.actorNumbersAtStart, m_DownloadSaveMetaData.dlcs);
		PFLog.Net.Log("Applying settings...");
		BaseSettingNetData[] settings = m_DownloadSaveMetaData.settings;
		for (int i = 0; i < settings.Length; i++)
		{
			settings[i].ForceSet();
		}
		PFLog.Net.Log("All settings was applied!");
		var (array, portraitsBytesCount) = PreparePortraitsGuidForDownload(m_DownloadSaveMetaData.portraitsSaveMeta, CustomPortraitsManager.Instance);
		m_Progress.PortraitsBytesCount = portraitsBytesCount;
		if (array.Length != 0)
		{
			PhotonManager.Instance.DataTransporter.CustomPortraitProgress += m_Progress.Report;
		}
		if (!SendMetaAck(player, array))
		{
			m_DownloadSaveTcs.SetException(new SendMessageFailException("Failed to send SaveMetaAcknowledge"));
		}
		static (Guid[] guids, int length) PreparePortraitsGuidForDownload(PortraitSaveMetaData[] masterPortraits, CustomPortraitsManager portraitsManager)
		{
			List<Guid> list = null;
			int num = 0;
			portraitsManager.FillAllPortraitsGuid();
			foreach (PortraitSaveMetaData portraitSaveMetaData in masterPortraits)
			{
				if (!portraitsManager.TryGetPortraitId(portraitSaveMetaData.guid, out var _))
				{
					if (list == null)
					{
						list = new List<Guid>();
					}
					list.Add(portraitSaveMetaData.guid);
					int[] imagesFileLength = portraitSaveMetaData.imagesFileLength;
					foreach (int num2 in imagesFileLength)
					{
						num += num2;
					}
				}
			}
			if (list == null)
			{
				return (guids: Array.Empty<Guid>(), length: num);
			}
			return (guids: list.ToArray(), length: num);
		}
		static bool SendMetaAck(PhotonActorNumber player, Guid[] guidsForDownload)
		{
			ByteArraySlice byteArraySlice = NetMessageSerializer.SerializeToSlice(new SaveMetaAcknowledgeData
			{
				PortraitsGuid = guidsForDownload.Select(delegate(Guid g)
				{
					SaveMetaAcknowledgeData.GuidData result = default(SaveMetaAcknowledgeData.GuidData);
					result.Guid = g;
					result.AlreadyHave = false;
					return result;
				}).ToArray()
			});
			return PhotonManager.Instance.SendMessageTo(player, 4, byteArraySlice.Buffer, byteArraySlice.Offset, byteArraySlice.Count);
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

	private static void RepackAndSaveToFile(byte[] data, string filePath)
	{
		PFLog.Net.Log("Repacking data...");
		byte[] bytes = SavePacker.RepackReceivedSave(data);
		PFLog.Net.Log("Saving to file... " + filePath);
		string directoryName = Path.GetDirectoryName(filePath);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		File.WriteAllBytes(filePath, bytes);
	}

	public void PostLoad()
	{
		if (m_OriginIdGuidPortraitsData.Count == 0)
		{
			return;
		}
		foreach (Entity allEntityDatum in Game.Instance.Player.CrossSceneState.AllEntityData)
		{
			PartUnitUISettings optional = allEntityDatum.GetOptional<PartUnitUISettings>();
			PortraitData portraitData = optional?.CustomPortraitRaw;
			if (portraitData != null && portraitData.IsCustom && m_OriginIdGuidPortraitsData.TryGetValue(optional.CustomPortraitRaw.CustomId, out var value))
			{
				CustomPortraitsManager.Instance.TryGetPortraitId(value, out var id);
				PFLog.Net.Log("[SaveNetManager.PostLoad] replace portrait " + optional.CustomPortraitRaw.CustomId + " -> " + id);
				optional.SetPortrait(new PortraitData(id), raiseEvent: false);
			}
		}
		m_OriginIdGuidPortraitsData.Clear();
	}
}
