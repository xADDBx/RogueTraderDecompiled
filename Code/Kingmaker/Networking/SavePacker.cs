using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Networking.Save;
using Kingmaker.Networking.Tools;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Serialization;
using Newtonsoft.Json;

namespace Kingmaker.Networking;

public static class SavePacker
{
	public static ArraySegment<byte> RepackSaveToSend(SaveInfo saveInfo)
	{
		bool shouldChangeSaveType = !SaveManager.IsCoopSave(saveInfo);
		using (saveInfo.GetReadScope())
		{
			return RepackSaveToSend(saveInfo.FolderName, null, shouldChangeSaveType);
		}
	}

	public static ArraySegment<byte> RepackSaveToSend(string savePath, [CanBeNull] string unzippedHeader = null, bool shouldChangeSaveType = false)
	{
		using FileStream stream = File.Open(savePath, FileMode.Open, FileAccess.Read, FileShare.Read);
		using ZipArchive inZipArchive = new ZipArchive(stream, ZipArchiveMode.Read);
		return RepackSaveToSend(inZipArchive, unzippedHeader, shouldChangeSaveType);
	}

	public static SaveInfoShort RepackSaveInfoShortToSend(SaveInfoShort saveInfoShort)
	{
		return new SaveInfoShort(saveInfoShort.Name, saveInfoShort.Area, saveInfoShort.AreaNameOverride, RepackPortraitsToGuid(saveInfoShort.PartyPortraits), saveInfoShort.SystemSaveTime, saveInfoShort.GameTotalTime);
	}

	public static SaveInfoShort RepackReceivedSaveInfoShort(SaveInfoShort saveInfoShort)
	{
		CustomPortraitsManager.Instance.FillAllPortraitsGuid();
		return new SaveInfoShort(saveInfoShort.Name, saveInfoShort.Area, saveInfoShort.AreaNameOverride, RepackPortraitsFromGuid(saveInfoShort.PartyPortraits), saveInfoShort.SystemSaveTime, saveInfoShort.GameTotalTime);
	}

	public static bool TryGetGuidFromPortrait(PortraitData portraitData, out Guid guid)
	{
		if (portraitData.IsCustom)
		{
			guid = CustomPortraitsManager.Instance.GetOrCreatePortraitGuid(portraitData.CustomId);
			return true;
		}
		guid = Guid.Empty;
		return false;
	}

	public static bool TryGetPortraitIdFromGuid(Guid guid, out string portraitId)
	{
		CustomPortraitsManager.Instance.FillAllPortraitsGuid();
		bool num = CustomPortraitsManager.Instance.TryGetPortraitId(guid, out portraitId);
		if (!num)
		{
			portraitId = CustomPortraitsManager.InvalidPortraitId;
		}
		return num;
	}

	private static ArraySegment<byte> RepackSaveToSend(ZipArchive inZipArchive, [CanBeNull] string unzippedHeader, bool shouldChangeSaveType)
	{
		JsonSerializer serializer = SaveSystemJsonSerializer.Serializer;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (ZipArchive zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
			{
				FogOfWarAreaCompressor fogOfWarAreaCompressor = new FogOfWarAreaCompressor(FindOptimalCompressorCapacity(inZipArchive.Entries));
				foreach (ZipArchiveEntry entry in inZipArchive.Entries)
				{
					if (IsScreenshotFile(entry.Name) || (IsHeaderFile(entry.Name) && !string.IsNullOrEmpty(unzippedHeader)))
					{
						continue;
					}
					using Stream stream2 = zipArchive.CreateEntry(entry.FullName).Open();
					using Stream stream = entry.Open();
					if (IsFogFile(entry.Name))
					{
						fogOfWarAreaCompressor.Compress(stream, stream2);
					}
					else if (IsHeaderFile(entry.Name))
					{
						ProcessHeader(serializer.DeserializeObject<SaveInfo>(stream), stream2);
					}
					else
					{
						stream.CopyTo(stream2);
					}
				}
				if (!string.IsNullOrEmpty(unzippedHeader))
				{
					using Stream outStream2 = zipArchive.CreateEntry("header.json").Open();
					ProcessHeader(serializer.DeserializeObject<SaveInfo>(unzippedHeader), outStream2);
				}
			}
			return new ArraySegment<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
		}
		void ProcessHeader(SaveInfo save, Stream outStream)
		{
			if (shouldChangeSaveType)
			{
				save.Name = "net_save_4b31b8ad92353a02";
				save.Type = SaveInfo.SaveType.Coop;
			}
			save.PartyPortraits = RepackPortraitsToGuid(save.PartyPortraits);
			serializer.SerializeObject(save, outStream);
		}
	}

	private static long FindOptimalCompressorCapacity(IEnumerable<ZipArchiveEntry> entries)
	{
		long num = 0L;
		foreach (ZipArchiveEntry entry in entries)
		{
			if (!IsScreenshotFile(entry.Name) && IsFogFile(entry.Name) && entry.Length > num)
			{
				num = entry.Length;
			}
		}
		return num;
	}

	public static byte[] RepackReceivedSave(byte[] saveBytes)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (ZipArchive zipArchive2 = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
		{
			using ZipArchive zipArchive = new ZipArchive(new MemoryStream(saveBytes), ZipArchiveMode.Read);
			FogOfWarAreaCompressor fogOfWarAreaCompressor = new FogOfWarAreaCompressor(FindOptimalDecompressorCapacity(zipArchive.Entries));
			foreach (ZipArchiveEntry entry in zipArchive.Entries)
			{
				using Stream stream2 = zipArchive2.CreateEntry(entry.FullName).Open();
				using Stream stream = entry.Open();
				if (IsFogFile(entry.Name))
				{
					fogOfWarAreaCompressor.Uncompress(stream, stream2);
				}
				else if (IsHeaderFile(entry.Name))
				{
					SaveInfo saveInfo = SaveSystemJsonSerializer.Serializer.DeserializeObject<SaveInfo>(stream);
					saveInfo.PartyPortraits = RepackPortraitsFromGuid(saveInfo.PartyPortraits);
					SaveSystemJsonSerializer.Serializer.SerializeObject(saveInfo, stream2);
				}
				else
				{
					stream.CopyTo(stream2);
				}
			}
		}
		return memoryStream.GetBuffer().SubArray(0, (int)memoryStream.Length);
	}

	private static long FindOptimalDecompressorCapacity(IEnumerable<ZipArchiveEntry> entries)
	{
		byte[] array = new byte[4];
		long num = 0L;
		foreach (ZipArchiveEntry entry in entries)
		{
			string name = entry.Name;
			if (name == "header.png" || name == "highres.png" || !IsFogFile(entry.Name))
			{
				continue;
			}
			using Stream stream = entry.Open();
			stream.Read(array, 0, 4);
			int num2 = BinaryPrimitives.ReadInt32BigEndian(array);
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	private static List<PortraitForSave> RepackPortraitsToGuid(List<PortraitForSave> partyPortraits)
	{
		List<PortraitForSave> list = new List<PortraitForSave>(partyPortraits.Count);
		foreach (PortraitForSave partyPortrait in partyPortraits)
		{
			if (partyPortrait.Data.IsCustom)
			{
				Guid guid = default(Guid);
				if (CustomPortraitsManager.Instance.HasPortraits(partyPortrait.Data.CustomId))
				{
					guid = CustomPortraitsManager.Instance.GetOrCreatePortraitGuid(partyPortrait.Data.CustomId);
				}
				else
				{
					PFLog.Net.Warning("[SavePacker.RepackPortraitsToGuid] " + partyPortrait.Data.CustomId + " not found, add default guid");
				}
				list.Add(new PortraitForSave(new PortraitData(guid.ToString()), partyPortrait.IsMainCharacter));
			}
			else
			{
				list.Add(partyPortrait);
			}
		}
		return list;
	}

	private static List<PortraitForSave> RepackPortraitsFromGuid(List<PortraitForSave> partyPortraits)
	{
		List<PortraitForSave> list = new List<PortraitForSave>(partyPortraits.Count);
		foreach (PortraitForSave partyPortrait in partyPortraits)
		{
			if (partyPortrait.Data.IsCustom)
			{
				string id = CustomPortraitsManager.InvalidPortraitId;
				try
				{
					if (!Guid.TryParse(partyPortrait.Data.CustomId, out var result))
					{
						PFLog.Net.Error("[SavePacker.RepackPortraitsFromGuid] can't parse guid '" + partyPortrait.Data.CustomId + "'");
					}
					else if (result == default(Guid))
					{
						PFLog.Net.Log("[SavePacker.RepackPortraitsFromGuid] source player alread doesn't have portrait");
					}
					else if (!CustomPortraitsManager.Instance.TryGetPortraitId(result, out id))
					{
						id = CustomPortraitsManager.InvalidPortraitId;
						PFLog.Net.Error($"[SavePacker.RepackPortraitsFromGuid] can't find portrait {result}");
					}
				}
				finally
				{
					list.Add(new PortraitForSave(new PortraitData(id), partyPortrait.IsMainCharacter));
				}
			}
			else
			{
				list.Add(partyPortrait);
			}
		}
		return list;
	}

	private static bool IsScreenshotFile(string name)
	{
		if (!(name == "header.png"))
		{
			return name == "highres.png";
		}
		return true;
	}

	private static bool IsFogFile(string name)
	{
		return name.EndsWith(".fog", StringComparison.Ordinal);
	}

	private static bool IsHeaderFile(string name)
	{
		return name == "header.json";
	}
}
