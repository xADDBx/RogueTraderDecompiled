using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
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
			return RepackSaveToSend(saveInfo.FolderName, shouldChangeSaveType);
		}
	}

	public static ArraySegment<byte> RepackSaveToSend(string savePath, bool shouldChangeSaveType = false)
	{
		using FileStream stream = File.Open(savePath, FileMode.Open, FileAccess.Read, FileShare.Read);
		using ZipArchive inZipArchive = new ZipArchive(stream, ZipArchiveMode.Read);
		return RepackSaveToSend(inZipArchive, shouldChangeSaveType);
	}

	private static ArraySegment<byte> RepackSaveToSend(ZipArchive inZipArchive, bool shouldChangeSaveType)
	{
		JsonSerializer serializer = SaveSystemJsonSerializer.Serializer;
		using MemoryStream memoryStream = new MemoryStream();
		using (ZipArchive zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
		{
			FogOfWarAreaCompressor fogOfWarAreaCompressor = new FogOfWarAreaCompressor(FindOptimalCompressorCapacity(inZipArchive.Entries));
			foreach (ZipArchiveEntry entry in inZipArchive.Entries)
			{
				if (IsScreenshotFile(entry.Name))
				{
					continue;
				}
				using Stream stream2 = zipArchive.CreateEntry(entry.FullName).Open();
				using Stream stream = entry.Open();
				if (IsFogFile(entry.Name))
				{
					fogOfWarAreaCompressor.Compress(stream, stream2);
				}
				else if (shouldChangeSaveType && IsHeaderFile(entry.Name))
				{
					SaveInfo saveInfo = serializer.DeserializeObject<SaveInfo>(stream);
					saveInfo.Name = "net_save_4b31b8ad92353a02";
					saveInfo.Type = SaveInfo.SaveType.Coop;
					serializer.SerializeObject(saveInfo, stream2);
				}
				else
				{
					stream.CopyTo(stream2);
				}
			}
		}
		return new ArraySegment<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
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
