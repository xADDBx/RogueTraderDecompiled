using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;
using Steamworks;
using UnityEngine;

namespace Kingmaker.Modding;

public class SteamWorkshopIntegration
{
	public class WriteableSettingsData
	{
		[JsonProperty]
		public string[] SourceDirectories = new string[0];

		[JsonProperty]
		public string[] EnabledModifications = new string[0];
	}

	private static bool InitSucceeded = false;

	private static string[] UnityModManagerFileNames = new string[4] { "dnlib.dll", "Ionic.Zip.dll", "UnityModManager.dll", "UnityModManager.pdb" };

	internal WriteableSettingsData m_Settings;

	public Dictionary<PublishedFileId_t, SteamWorkshopModification> SubscribedModifications = new Dictionary<PublishedFileId_t, SteamWorkshopModification>();

	private static SteamWorkshopIntegration s_Instance;

	public static bool Started { get; private set; }

	private static string SettingsFilePath => Path.Combine(ApplicationPaths.persistentDataPath, "OwlcatModificationManagerSettings.json");

	internal static string DefaultOwlcatTemplateDirectory => Path.Combine(ApplicationPaths.persistentDataPath, "Modifications");

	internal static string DefaultUMMDirectory => Path.Combine(ApplicationPaths.persistentDataPath, "UnityModManager");

	private static string UnityModManagerFilesZipPath => Path.Combine(ApplicationPaths.streamingAssetsPath, "OwlcatUnityModManager.zip");

	public static SteamWorkshopIntegration Instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = new SteamWorkshopIntegration();
			}
			return s_Instance;
		}
	}

	private SteamWorkshopIntegration()
	{
		if (!SteamAPI.Init())
		{
			PFLog.Mods.Log("Could not initialize SteamAPI.");
			return;
		}
		if (!SteamUser.BLoggedOn())
		{
			PFLog.Mods.Log("Could not connect to Steam Servers. User is either not logged in or Servers are down.");
			return;
		}
		PFLog.Mods.Log("Starting Steam Workshop Integration.");
		try
		{
			EnsureUnityModManagerFiles();
			EnsureTemplateDirectoryAndSettingsFile();
		}
		catch (Exception ex)
		{
			PFLog.Mods.Exception(ex);
			return;
		}
		InitSucceeded = true;
	}

	public void Start()
	{
		if (Started || !InitSucceeded)
		{
			return;
		}
		Started = true;
		try
		{
			GetSubscribedModifications();
			GetInstalledModifications();
			InstallOrUpdateSubscribedModifications();
			File.WriteAllText(SettingsFilePath, JsonUtility.ToJson(m_Settings, prettyPrint: true));
		}
		catch (Exception ex)
		{
			PFLog.Mods.Exception(ex);
		}
	}

	private void InstallOrUpdateSubscribedModifications()
	{
		foreach (SteamWorkshopModification value in SubscribedModifications.Values)
		{
			if (!value.IsDownloading)
			{
				if (value.WorkshopSettings == null)
				{
					value.Install();
				}
				else if (value.LocalTimestamp < value.SteamTimestamp)
				{
					value.Install(isUpdate: true);
				}
			}
		}
	}

	private void GetInstalledModifications()
	{
		string[] directories = Directory.GetDirectories(DefaultUMMDirectory);
		foreach (string text in directories)
		{
			FileInfo fileInfo = new FileInfo(Path.Combine(text, "WorkshopManaged.json"));
			if (!fileInfo.Exists)
			{
				continue;
			}
			try
			{
				SteamWorkshopModification.WorkshopSettingsData workshopSettingsData = JsonUtility.FromJson<SteamWorkshopModification.WorkshopSettingsData>(File.ReadAllText(fileInfo.FullName));
				if (workshopSettingsData == null)
				{
					throw new Exception();
				}
				if (SubscribedModifications.TryGetValue(workshopSettingsData.SteamFileId, out var value))
				{
					value.WorkshopSettings = workshopSettingsData;
				}
				else
				{
					UninstallModification(text, isUmm: true, workshopSettingsData.UniqueName);
				}
			}
			catch (Exception)
			{
				UninstallModification(text, isUmm: true);
			}
		}
		directories = Directory.GetDirectories(DefaultOwlcatTemplateDirectory);
		foreach (string text2 in directories)
		{
			FileInfo fileInfo2 = new FileInfo(Path.Combine(text2, "WorkshopManaged.json"));
			if (!fileInfo2.Exists)
			{
				continue;
			}
			try
			{
				SteamWorkshopModification.WorkshopSettingsData workshopSettingsData2 = JsonUtility.FromJson<SteamWorkshopModification.WorkshopSettingsData>(File.ReadAllText(fileInfo2.FullName));
				if (workshopSettingsData2 == null)
				{
					throw new Exception();
				}
				if (SubscribedModifications.TryGetValue(workshopSettingsData2.SteamFileId, out var value2))
				{
					value2.WorkshopSettings = workshopSettingsData2;
				}
				else
				{
					UninstallModification(text2, isUmm: false, workshopSettingsData2.UniqueName);
				}
			}
			catch (Exception)
			{
				UninstallModification(text2, isUmm: false);
			}
		}
	}

	private void UninstallModification(string ModificationDirectory, bool isUmm, string UniqueName = null)
	{
		PFLog.Mods.Log("Deleting unsubscribed modification: " + (UniqueName ?? "Unknown Modification") + " at " + ModificationDirectory);
		try
		{
			Directory.Delete(ModificationDirectory, recursive: true);
			if (!isUmm && !UniqueName.IsNullOrEmpty())
			{
				List<string> list = m_Settings.EnabledModifications.ToList();
				list.Remove(UniqueName);
				m_Settings.EnabledModifications = list.ToArray();
			}
		}
		catch (Exception ex)
		{
			PFLog.Mods.Exception(ex);
		}
	}

	private void GetSubscribedModifications()
	{
		uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
		PublishedFileId_t[] array = new PublishedFileId_t[numSubscribedItems];
		SteamUGC.GetSubscribedItems(array, numSubscribedItems);
		PublishedFileId_t[] array2 = array;
		foreach (PublishedFileId_t publishedFileId_t in array2)
		{
			SubscribedModifications[publishedFileId_t] = new SteamWorkshopModification(publishedFileId_t);
		}
	}

	private void EnsureUnityModManagerFiles()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(DefaultUMMDirectory);
		if (directoryInfo.Exists)
		{
			HashSet<string> hashSet = new HashSet<string>(from f in directoryInfo.GetFiles()
				select f.Name);
			if (!UnityModManagerFileNames.All(hashSet.Contains))
			{
				ReinstallUnityModManager();
			}
		}
		else
		{
			directoryInfo.Create();
			ReinstallUnityModManager();
		}
	}

	private void ReinstallUnityModManager()
	{
		PFLog.Mods.Log("Copying UnityModManager Files from " + UnityModManagerFilesZipPath + " to " + DefaultUMMDirectory + ".");
		DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "RogueTraderUMMAssets"));
		try
		{
			if (directoryInfo.Exists)
			{
				directoryInfo.Delete(recursive: true);
			}
			directoryInfo.Create();
			ZipFile.ExtractToDirectory(UnityModManagerFilesZipPath, directoryInfo.FullName);
			string[] unityModManagerFileNames = UnityModManagerFileNames;
			foreach (string text in unityModManagerFileNames)
			{
				FileInfo fileInfo = new FileInfo(Path.Combine(DefaultUMMDirectory, text));
				if (fileInfo.Exists)
				{
					fileInfo.Delete();
				}
				new FileInfo(Path.Combine(directoryInfo.FullName, "UnityModManager", text)).MoveTo(fileInfo.FullName);
			}
		}
		finally
		{
			if (directoryInfo.Exists)
			{
				directoryInfo.Delete(recursive: true);
			}
		}
	}

	private void EnsureTemplateDirectoryAndSettingsFile()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(DefaultOwlcatTemplateDirectory);
		if (!directoryInfo.Exists)
		{
			directoryInfo.Create();
		}
		if (File.Exists(SettingsFilePath))
		{
			try
			{
				m_Settings = JsonUtility.FromJson<WriteableSettingsData>(File.ReadAllText(SettingsFilePath));
				if (m_Settings == null)
				{
					throw new Exception();
				}
				return;
			}
			catch (Exception)
			{
				File.Delete(SettingsFilePath);
				RecreateTemplateModSettingsFile();
				return;
			}
		}
		RecreateTemplateModSettingsFile();
	}

	private void RecreateTemplateModSettingsFile()
	{
		PFLog.Mods.Log("Creating default OwlcatModificationManagerSettings.json in " + SettingsFilePath);
		m_Settings = new WriteableSettingsData();
		File.WriteAllText(SettingsFilePath, JsonUtility.ToJson(m_Settings, prettyPrint: true));
	}
}
