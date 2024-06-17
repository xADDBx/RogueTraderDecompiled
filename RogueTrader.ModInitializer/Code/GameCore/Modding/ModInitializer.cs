using System;
using System.IO;
using Kingmaker;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase.ResourceReplacementProvider;
using Kingmaker.GameInfo;
using Kingmaker.Localization;
using Kingmaker.Modding;
using Kingmaker.Stores;
using Kingmaker.Utility.ModsInfo;
using UnityEngine;

namespace Code.GameCore.Modding;

public static class ModInitializer
{
	private const string ActiveUmmItemsInfo = "ActiveUMMItemsInfo.txt";

	private static readonly string ActiveModFilePath = Path.Combine(Application.persistentDataPath, "ActiveUMMItemsInfo.txt");

	public static UserModsData UserModsData => UserModsData.Instance;

	public static IResourceReplacementProvider ResourceReplacementProvider
	{
		get
		{
			if (!OwlcatModificationsManager.Instance.Started)
			{
				return null;
			}
			return OwlcatModificationsManager.Instance;
		}
	}

	public static void InitializeMods()
	{
		PrepareModUsageInfoFile();
		if (CheckSteam())
		{
			SteamManager instance = SteamManager.Instance;
			if (!SteamManager.Initialized)
			{
				return;
			}
			new SteamWorkshopIntegration(instance).Start();
		}
		InitializeUnityModManager();
		InitializeOwlcatModManager();
		GetUsedModsInfo();
	}

	public static void ApplyOwlcatModificationsContent()
	{
		OwlcatModificationsManager.Instance.ApplyModificationsContent();
	}

	public static void InitializeModsUI()
	{
		UnityModManagerAdapter.Instance.TryStartUI();
	}

	public static void GetUsedModsInfo()
	{
		UserModsData instance = UserModsData.Instance;
		UserModsData.Instance.ExternalUmmUsed = UnityModManagerAdapter.Instance.ExternalUmmUsed;
		if (!File.Exists(ActiveModFilePath))
		{
			PFLog.Mods.Log($"Mods info: used mods count -{UserModsData.Instance.UsedMods.Count}, external umm - {UserModsData.Instance.ExternalUmmUsed}, playing with mods - {UserModsData.Instance.PlayingWithMods}");
			return;
		}
		try
		{
			using StreamReader streamReader = File.OpenText(ActiveModFilePath);
			string empty = string.Empty;
			while ((empty = streamReader.ReadLine()) != null)
			{
				string[] array = empty.Split("~~~");
				if (array != null && array.Length == 2)
				{
					instance.UsedMods.Add(new ModInfo
					{
						Id = array[0],
						Version = array[1]
					});
				}
			}
		}
		catch (Exception ex)
		{
			PFLog.Mods.Exception(ex);
		}
		PFLog.Mods.Log($"Mods info: used mods count -{UserModsData.Instance.UsedMods.Count}, external umm - {UserModsData.Instance.ExternalUmmUsed}, playing with mods - {UserModsData.Instance.PlayingWithMods}");
	}

	private static bool CheckSteam()
	{
		return StoreManager.Store == StoreType.Steam;
	}

	private static void PrepareModUsageInfoFile()
	{
		if (File.Exists(ActiveModFilePath))
		{
			File.Delete(ActiveModFilePath);
		}
	}

	private static void InitializeUnityModManager()
	{
		PFLog.Mods.Log("Starting OwlcatUnityModManager");
		UnityModManagerAdapter.Instance.TryStart(GameVersion.GetVersion());
		PFLog.Mods.Log(" UnityModManagerAdapter.Instance.TryStart");
	}

	private static void InitializeOwlcatModManager()
	{
		PFLog.Mods.Log("Starting Owlcat Mod Manager");
		OwlcatModificationsManager.Instance.Start(LocalizationManager.Instance);
		PFLog.Mods.Log("Owlcat Mod Manager finished initialization");
	}
}
