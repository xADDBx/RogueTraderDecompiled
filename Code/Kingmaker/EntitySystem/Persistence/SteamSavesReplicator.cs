using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.GameInfo;
using Kingmaker.Stores;
using Kingmaker.Utility.BuildModeUtils;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using Steamworks;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence;

public class SteamSavesReplicator
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("Steam");

	private const string SteamSavesFile = "steam-saves-release.json";

	private volatile bool m_Initialized;

	[NotNull]
	private string m_PersistentDataPath = "";

	[NotNull]
	private SteamSavesRegistry m_Registry = new SteamSavesRegistry();

	[Cheat(Name = "steam_force_full_download")]
	public static bool ForceFullDownload { get; set; }

	public void RegisterSave(SaveInfo saveInfo)
	{
		if (GameVersion.Mode != BuildMode.Betatest && StoreManager.Store == StoreType.Steam)
		{
			Initialize();
			Task.Run(delegate
			{
				RegisterSaveThreaded(saveInfo);
			});
		}
	}

	public void PullUpdates()
	{
		if (GameVersion.Mode != BuildMode.Betatest && StoreManager.Store == StoreType.Steam)
		{
			PullUpdatesImpl();
		}
	}

	public void DeleteSave(SaveInfo saveInfo)
	{
		if (GameVersion.Mode != BuildMode.Betatest && StoreManager.Store == StoreType.Steam)
		{
			Initialize();
			string fileName = saveInfo.FileName;
			Task.Run(delegate
			{
				DeleteSaveThreaded(fileName);
			});
		}
	}

	private void RegisterSaveThreaded(SaveInfo saveInfo)
	{
		if (GameVersion.Mode == BuildMode.Betatest)
		{
			return;
		}
		try
		{
			lock (this)
			{
				if (saveInfo.IsActuallySaved && saveInfo.FileName.EndsWith(".zks"))
				{
					byte[] array;
					using (saveInfo.GetReadScope())
					{
						array = File.ReadAllBytes(saveInfo.FolderName);
					}
					EnsureCloudSpace((ulong)array.Length);
					if (SteamRemoteStorage.FileWrite(saveInfo.FileName, array, array.Length))
					{
						Logger.Log(saveInfo.FileName + " : uploaded to cloud");
					}
					else
					{
						Logger.Error(saveInfo.FileName + " : failed to upload to cloud");
					}
					m_Registry.RegisterSave(saveInfo.FileName);
					UploadRegistry();
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Exception(ex);
		}
	}

	private static void EnsureCloudSpace(ulong requiredBytes)
	{
		if (GameVersion.Mode == BuildMode.Betatest)
		{
			return;
		}
		requiredBytes += 1048576;
		SteamRemoteStorage.GetQuota(out var pnTotalBytes, out var puAvailableBytes);
		if (puAvailableBytes >= requiredBytes)
		{
			return;
		}
		List<SaveFileInfo> list = new List<SaveFileInfo>();
		int fileCount = SteamRemoteStorage.GetFileCount();
		for (int i = 0; i < fileCount; i++)
		{
			int pnFileSizeInBytes;
			string fileNameAndSize = SteamRemoteStorage.GetFileNameAndSize(i, out pnFileSizeInBytes);
			long fileTimestamp = SteamRemoteStorage.GetFileTimestamp(fileNameAndSize);
			SaveFileInfo item = new SaveFileInfo
			{
				FileId = i,
				FileName = fileNameAndSize,
				FileTimestamp = fileTimestamp,
				FileSize = pnFileSizeInBytes
			};
			list.Add(item);
		}
		list.Sort();
		ulong num = puAvailableBytes;
		foreach (SaveFileInfo item2 in list)
		{
			if (!item2.FileName.EndsWith(".json"))
			{
				if (SteamRemoteStorage.FileDelete(item2.FileName))
				{
					Logger.Log(item2.FileName + " : deleted");
					num += (uint)item2.FileSize;
				}
				else
				{
					Logger.Error(item2.FileName + " : failed to delete");
				}
				if (num >= requiredBytes)
				{
					SteamRemoteStorage.GetQuota(out pnTotalBytes, out puAvailableBytes);
					Logger.Log("Successfully freed some quota space " + $"| requiredBytes = {requiredBytes} " + $"| freeBytes = {puAvailableBytes} " + $"| expectedFreeBytes = {num} " + $"| totalBytes = {pnTotalBytes}");
					break;
				}
			}
		}
	}

	private void PullUpdatesImpl()
	{
		if (GameVersion.Mode == BuildMode.Betatest)
		{
			return;
		}
		try
		{
			lock (this)
			{
				SteamSavesRegistry steamSavesRegistry;
				if (!SteamRemoteStorage.FileExists("steam-saves-release.json"))
				{
					steamSavesRegistry = new SteamSavesRegistry();
				}
				else
				{
					int fileSize = SteamRemoteStorage.GetFileSize("steam-saves-release.json");
					byte[] array = new byte[fileSize];
					SteamRemoteStorage.FileRead("steam-saves-release.json", array, fileSize);
					steamSavesRegistry = JsonConvert.DeserializeObject<SteamSavesRegistry>(Encoding.UTF8.GetString(array));
					Logger.Log($"Downloaded remote registry (version: {steamSavesRegistry.Version})");
				}
				if (steamSavesRegistry.Version <= m_Registry.Version && !ForceFullDownload)
				{
					return;
				}
				Logger.Log($"Downloading changed files (local version: {m_Registry.Version}, remote version: {steamSavesRegistry.Version})");
				foreach (string item in ForceFullDownload ? steamSavesRegistry.Files.Select((SteamSaveFile v) => v.Filename).ToList() : SteamSavesRegistry.GetChangedFiles(m_Registry, steamSavesRegistry))
				{
					if (!SteamRemoteStorage.FileExists(item))
					{
						continue;
					}
					try
					{
						int fileSize2 = SteamRemoteStorage.GetFileSize(item);
						byte[] array2 = new byte[fileSize2];
						SteamRemoteStorage.FileRead(item, array2, fileSize2);
						Logger.Log("Downloaded " + item + " from cloud");
						string path = Path.Combine(m_PersistentDataPath, "Saved Games", item);
						SaveInfo saveByFile = Game.Instance.SaveManager.GetSaveByFile(item);
						using (saveByFile?.GetWriteScope())
						{
							File.WriteAllBytes(path, array2);
							Logger.Log("Written " + item + " to disk");
							Game.Instance.SaveManager.RemoveSaveFromList(saveByFile);
						}
					}
					catch (Exception ex)
					{
						Logger.Error(ex);
					}
				}
				m_Registry = steamSavesRegistry;
				SaveRegistry();
			}
		}
		catch (Exception ex2)
		{
			Logger.Exception(ex2);
		}
	}

	private void DeleteSaveThreaded(string fileName)
	{
		if (GameVersion.Mode == BuildMode.Betatest)
		{
			return;
		}
		try
		{
			lock (this)
			{
				if (fileName.EndsWith(".zks"))
				{
					if (SteamRemoteStorage.FileDelete(fileName))
					{
						Logger.Log(fileName + " : deleted from cloud");
					}
					else
					{
						Logger.Error(fileName + " : failed to delete from cloud");
					}
					m_Registry.DeleteSave(fileName);
					UploadRegistry();
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Error(ex);
		}
	}

	private void SaveRegistry()
	{
		string s = JsonConvert.SerializeObject(m_Registry);
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		File.WriteAllBytes(Path.Combine(m_PersistentDataPath, "steam-saves-release.json"), bytes);
		Logger.Log($"Written registry to disk (version: {m_Registry.Version})");
	}

	private void UploadRegistry()
	{
		string s = JsonConvert.SerializeObject(m_Registry);
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		if (SteamRemoteStorage.FileWrite("steam-saves-release.json", bytes, bytes.Length))
		{
			Logger.Log($"Uploaded registry to cloud (version: {m_Registry.Version})");
		}
		else
		{
			Logger.Error($"Failed to upload registry to cloud (version: {m_Registry.Version})");
		}
		File.WriteAllBytes(Path.Combine(m_PersistentDataPath, "steam-saves-release.json"), bytes);
		Logger.Log($"Written registry to disk (version: {m_Registry.Version})");
	}

	public void Initialize()
	{
		if (GameVersion.Mode == BuildMode.Betatest || m_Initialized)
		{
			return;
		}
		m_PersistentDataPath = Application.persistentDataPath;
		string path = Path.Combine(m_PersistentDataPath, "steam-saves-release.json");
		if (File.Exists(path))
		{
			try
			{
				string value = File.ReadAllText(path);
				m_Registry = JsonConvert.DeserializeObject<SteamSavesRegistry>(value);
				Logger.Log($"Loaded registry (version: {m_Registry.Version})");
			}
			catch (Exception)
			{
				Logger.Error("Failed to load steam saves registry");
				m_Registry = new SteamSavesRegistry();
				Logger.Log("Created new registry");
			}
		}
		else
		{
			m_Registry = new SteamSavesRegistry();
			Logger.Log("Created new registry");
		}
		m_Initialized = true;
	}
}
