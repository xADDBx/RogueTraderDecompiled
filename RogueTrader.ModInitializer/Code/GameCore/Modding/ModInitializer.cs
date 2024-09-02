using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Code.Utility.ExtendedModInfo;
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

	private static List<ExtendedModInfo> s_AllModsCache = new List<ExtendedModInfo>();

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

	public static void CheckForModUpdates()
	{
		UnityModManagerAdapter.Instance.CheckForUpdates();
		OwlcatModificationsManager.Instance.CheckForUpdates();
	}

	public static void OpenModInfoWindow(string modId, bool forceUpdate = false)
	{
		GetAllModsInfo(forceUpdate: true);
		ExtendedModInfo extendedModInfo = FindMod(modId);
		if (extendedModInfo != null)
		{
			if (extendedModInfo.IsUmmMod)
			{
				UnityModManagerAdapter.Instance.OpenModInfoWindow(modId);
			}
			else
			{
				OwlcatModificationsManager.Instance.OpenModInfoWindow(modId);
			}
		}
	}

	public static ExtendedModInfo GetModInfo(string modId, bool forceUpdate = false)
	{
		GetAllModsInfo(forceUpdate);
		ExtendedModInfo extendedModInfo = FindMod(modId);
		if (extendedModInfo == null)
		{
			return null;
		}
		if (extendedModInfo.IsUmmMod)
		{
			return UnityModManagerAdapter.Instance.GetModInfo(modId);
		}
		return OwlcatModificationsManager.Instance.GetModInfo(modId);
	}

	public static List<ExtendedModInfo> GetAllModsInfo(bool forceUpdate = false)
	{
		if (!forceUpdate && s_AllModsCache.Count != 0)
		{
			return s_AllModsCache;
		}
		List<ExtendedModInfo> list = new List<ExtendedModInfo>();
		List<ExtendedModInfo> allModsInfo = OwlcatModificationsManager.Instance.GetAllModsInfo();
		List<ExtendedModInfo> allModsInfo2 = UnityModManagerAdapter.Instance.GetAllModsInfo();
		if (allModsInfo != null)
		{
			PFLog.Mods.Log($"OwlcatModificationsManager has {allModsInfo.Count} mods installed.");
			list.AddRange(allModsInfo);
		}
		else
		{
			PFLog.Mods.Log("OwlcatModificationsManager returned null instead of all mods.");
		}
		if (allModsInfo2 != null)
		{
			PFLog.Mods.Log($"UnityModificationsManager has {allModsInfo2.Count} mods installed.");
			list.AddRange(allModsInfo2);
		}
		else
		{
			PFLog.Mods.Log("UnityModificationsManager returned null instead of all mods.");
		}
		PFLog.Mods.Log("Mods information cache updated.");
		s_AllModsCache = list;
		return list;
	}

	public static void EnableMod(ExtendedModInfo mod, bool state)
	{
		EnableMod(mod.Id, state);
	}

	public static void EnableMod(string modId, bool state, bool forceUpdate = false)
	{
		GetAllModsInfo(forceUpdate);
		ExtendedModInfo extendedModInfo = FindMod(modId);
		if (extendedModInfo != null)
		{
			if (extendedModInfo.IsUmmMod)
			{
				UnityModManagerAdapter.Instance.EnableMod(modId, state);
			}
			else
			{
				OwlcatModificationsManager.Instance.EnableMod(modId, state);
			}
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

	private static ExtendedModInfo FindMod(string modId)
	{
		if (s_AllModsCache.Count == 0)
		{
			PFLog.Mods.Log("Trying to call OpenModInfoWindow while there are no mods detected");
			return null;
		}
		List<ExtendedModInfo> list = s_AllModsCache.Where((ExtendedModInfo x) => x.Id == modId).ToList();
		if (list.Count == 0)
		{
			PFLog.Mods.Error("Found no mod with id " + modId);
			return null;
		}
		if (list.Count > 1)
		{
			PFLog.Mods.Error("Found more than one mod with id " + modId);
			return null;
		}
		return list[0];
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
