using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Code.GameCore.Blueprints.BlueprintPatcher;
using Code.Utility.ExtendedModInfo;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase.ResourceReplacementProvider;
using Kingmaker.BundlesLoading;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Localization.Enums;
using Kingmaker.Localization.Shared;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.NewtonsoftJson;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.Modding;

public class OwlcatModificationsManager : IResourceReplacementProvider
{
	public class SettingsData
	{
		[JsonProperty]
		public readonly string[] SourceDirectories = new string[0];

		[JsonProperty]
		public readonly string[] EnabledModifications = new string[0];

		[JsonProperty]
		public List<string> ActiveModifications = new List<string>();

		[JsonProperty]
		public List<string> DisabledModifications = new List<string>();
	}

	public delegate void ShowModSettingsCalled(string modId);

	public const string SettingsFileName = "OwlcatModificationManagerSettings.json";

	private static OwlcatModificationsManager s_Instance;

	public static readonly JsonSerializer BlueprintPatchSerializer = JsonSerializer.Create(new JsonSerializerSettings
	{
		Formatting = Formatting.Indented
	});

	public ShowModSettingsCalled OnShowModSettingsCalled;

	public ShowModSettingsCalled OnShowModSettingsFromInGameUI;

	[CanBeNull]
	private readonly SettingsData m_Settings;

	private ILocalizationProvider m_LocalizationProvider;

	private OwlcatModification[] m_Modifications = new OwlcatModification[0];

	private bool m_UnityModManagerActive;

	public static OwlcatModificationsManager Instance => s_Instance ?? (s_Instance = new OwlcatModificationsManager());

	private static string SettingsFilePath => Path.Combine(ApplicationPaths.persistentDataPath, "OwlcatModificationManagerSettings.json");

	private static string DefaultModificationsDirectory => Path.Combine(ApplicationPaths.persistentDataPath, "Modifications");

	public bool Started { get; private set; }

	[NotNull]
	public OwlcatModification[] AppliedModifications { get; private set; } = new OwlcatModification[0];


	[NotNull]
	public OwlcatModification[] InstalledModifications { get; private set; } = new OwlcatModification[0];


	private OwlcatModificationsManager()
	{
		try
		{
			if (File.Exists(SettingsFilePath))
			{
				m_Settings = NewtonsoftJsonHelper.DeserializeFromFile<SettingsData>(SettingsFilePath);
				if (m_Settings != null)
				{
					PFLog.Mods.Log("Disabled Modifications ");
					foreach (string disabledModification in m_Settings.DisabledModifications)
					{
						PFLog.Mods.Log("Disabled : " + disabledModification);
					}
					PFLog.Mods.Log("Active modifications ");
					{
						foreach (string disabledModification2 in m_Settings.DisabledModifications)
						{
							PFLog.Mods.Log("Active : " + disabledModification2);
						}
						return;
					}
				}
				PFLog.Mods.Error("Settings file for OwlcatModificationManager failed to load.");
			}
			else
			{
				PFLog.Mods.Log("No modification settings file found.");
			}
		}
		catch (Exception ex)
		{
			PFLog.Mods.Exception(ex);
		}
	}

	public ExtendedModInfo GetModInfo(string modId)
	{
		List<OwlcatModification> list = InstalledModifications.Where((OwlcatModification x) => x.UniqueName == modId).ToList();
		if (list == null || list.Count == 1)
		{
			PFLog.Mods.Error("Mod with id " + modId + " not found in OWM");
			return null;
		}
		return list[0].ToExtendedModInfo();
	}

	public List<ExtendedModInfo> GetAllModsInfo()
	{
		List<ExtendedModInfo> list = new List<ExtendedModInfo>();
		OwlcatModification[] installedModifications = InstalledModifications;
		foreach (OwlcatModification mod in installedModifications)
		{
			list.Add(mod.ToExtendedModInfo());
		}
		return list;
	}

	public void CheckForUpdates()
	{
	}

	public void OpenModInfoWindow(string modId)
	{
		if (OnShowModSettingsFromInGameUI == null)
		{
			PFLog.Mods.Error("OwlcatModificationWindow has no subscription to OnShowModSettingsCalled event. ");
		}
		else
		{
			OnShowModSettingsFromInGameUI(modId);
		}
	}

	public void EnableMod(string modId, bool state)
	{
		List<OwlcatModification> list = InstalledModifications.Where((OwlcatModification x) => x.UniqueName == modId).ToList();
		if (list == null || list.Count != 1)
		{
			PFLog.Mods.Error("Mod with id " + modId + " not found in OWM");
			return;
		}
		OwlcatModification owlcatModification = list[0];
		if (owlcatModification.Enabled == state)
		{
			return;
		}
		owlcatModification.Enabled = state;
		if (m_Settings == null)
		{
			PFLog.Mods.Error("OwlcatModificationManager Settings are not initialized!");
			return;
		}
		if (owlcatModification.Enabled)
		{
			PFLog.Mods.Log("Enabling mod : " + owlcatModification.UniqueName);
			m_Settings.ActiveModifications.Add(owlcatModification.UniqueName);
			m_Settings.DisabledModifications.Remove(owlcatModification.UniqueName);
		}
		else
		{
			PFLog.Mods.Log("Disabling mod " + owlcatModification.UniqueName);
			m_Settings.ActiveModifications.Remove(owlcatModification.UniqueName);
			m_Settings.DisabledModifications.Add(owlcatModification.UniqueName);
		}
		owlcatModification.OnSetEnabled?.Invoke(state);
		SaveSettings();
	}

	public void SaveSettings()
	{
		PFLog.Mods.Log("Saving OwlcatModManager Settings changes.");
		if (File.Exists(SettingsFilePath))
		{
			File.Delete(SettingsFilePath);
		}
		using FileStream stream = File.Open(SettingsFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
		using StreamWriter streamWriter = new StreamWriter(stream);
		SaveSystemJsonSerializer.Serializer.Serialize(streamWriter, m_Settings);
		streamWriter.Flush();
	}

	public void Start(ILocalizationProvider localizationProvider)
	{
		if (!Started)
		{
			Started = true;
			m_LocalizationProvider = localizationProvider;
			try
			{
				LoadModifications();
				ApplyModifications();
				ApplyLocalization(localizationProvider.CurrentLocale);
				m_LocalizationProvider.LocaleChanged += ApplyLocalization;
			}
			catch (Exception ex)
			{
				PFLog.Mods.Exception(ex);
			}
			m_UnityModManagerActive = AppDomain.CurrentDomain.GetAssemblies().HasItem((Assembly i) => i.FullName.Contains("UnityModManager"));
			GenerateUsedModInfo();
		}
	}

	private void GenerateUsedModInfo()
	{
		string path = Path.Combine(Application.persistentDataPath, "ActiveUMMItemsInfo.txt");
		if (AppliedModifications == null || AppliedModifications.Length == 0)
		{
			return;
		}
		try
		{
			using StreamWriter streamWriter = new StreamWriter(path, append: true);
			OwlcatModification[] appliedModifications = AppliedModifications;
			foreach (OwlcatModification owlcatModification in appliedModifications)
			{
				streamWriter.WriteLine(owlcatModification.Manifest.UniqueName + "~~~" + owlcatModification.Manifest.Version);
			}
		}
		catch (Exception ex)
		{
			PFLog.Mods.Exception(ex);
		}
	}

	private void LoadModifications()
	{
		if (m_Settings == null)
		{
			return;
		}
		List<OwlcatModification> list = new List<OwlcatModification>();
		foreach (string item in m_Settings.SourceDirectories.Concat(DefaultModificationsDirectory))
		{
			string text = item;
			if (!Path.IsPathRooted(text))
			{
				text = Path.Combine(ApplicationPaths.persistentDataPath, text);
			}
			if (Directory.Exists(text))
			{
				PFLog.Mods.Log("Load modifications' descriptors: " + text);
				try
				{
					list.AddRange(LoadModifications(text));
				}
				catch (Exception ex)
				{
					PFLog.Mods.Exception(ex);
				}
			}
		}
		m_Modifications = list.ToArray();
	}

	private IEnumerable<OwlcatModification> LoadModifications(string directoryPath)
	{
		List<OwlcatModification> list = new List<OwlcatModification>();
		string[] directories = Directory.GetDirectories(directoryPath);
		foreach (string modificationDirectoryPath in directories)
		{
			list.Add(OwlcatModification.LoadFromDirectory(modificationDirectoryPath, directoryPath, m_LocalizationProvider));
		}
		return list;
	}

	private void ApplyModifications()
	{
		if (m_Settings == null)
		{
			return;
		}
		List<OwlcatModification> list = new List<OwlcatModification>();
		string[] enabledModifications = m_Settings.EnabledModifications;
		foreach (string modificationName in enabledModifications)
		{
			if (modificationName.IsNullOrEmpty())
			{
				PFLog.Mods.Error("Empty modification name in Settings.EnabledModifications");
				continue;
			}
			OwlcatModification owlcatModification = m_Modifications.FirstItem((OwlcatModification d) => d.Manifest?.UniqueName == modificationName);
			if (owlcatModification == null)
			{
				PFLog.Mods.Error("Missing modification: " + modificationName);
				continue;
			}
			string path = owlcatModification.Path;
			OwlcatModificationManifest manifest = owlcatModification.Manifest;
			if (manifest == null)
			{
				PFLog.Mods.Error("Modification can't be loaded: " + modificationName + " (" + path + ")");
			}
			else if (list.HasItem((OwlcatModification m) => m.Manifest?.UniqueName == modificationName))
			{
				PFLog.Mods.Error("Modification with same name already loaded: " + modificationName);
			}
			else if (CheckDependencies(manifest, list))
			{
				PFLog.Mods.Log("Apply modification: " + manifest.UniqueName + " (" + path + ")");
				owlcatModification.Apply();
				list.Add(owlcatModification);
			}
		}
		List<string> list2 = new List<string>();
		foreach (OwlcatModification item in list)
		{
			string modId = item.UniqueName;
			if (m_Settings.DisabledModifications.Contains((string mod) => mod == modId))
			{
				PFLog.Mods.Error("Disabled contains " + modId);
				item.Enabled = false;
				list2.Add(item.UniqueName);
			}
			else if (m_Settings.ActiveModifications.Contains((string mod) => mod == modId))
			{
				item.Enabled = true;
			}
			else
			{
				item.Enabled = true;
				m_Settings.ActiveModifications.Add(item.UniqueName);
			}
		}
		InstalledModifications = list.ToArray();
		foreach (string modId in list2)
		{
			AppliedModifications.Remove((OwlcatModification x) => x.UniqueName == modId);
		}
		AppliedModifications = list.ToArray();
		SaveSettings();
	}

	public void ApplyModificationsContent()
	{
		OwlcatModification[] appliedModifications = AppliedModifications;
		for (int i = 0; i < appliedModifications.Length; i++)
		{
			appliedModifications[i].ApplyContent();
		}
	}

	public void ApplyLocalization(Locale _)
	{
		if (m_LocalizationProvider.CurrentPack != null)
		{
			OwlcatModification[] appliedModifications = AppliedModifications;
			for (int i = 0; i < appliedModifications.Length; i++)
			{
				appliedModifications[i].ApplyLocalization();
			}
		}
	}

	private static bool CheckDependencies(OwlcatModificationManifest manifest, List<OwlcatModification> appliedModifications)
	{
		string uniqueName = manifest.UniqueName;
		bool result = true;
		OwlcatModificationManifest.Dependency[] dependencies = manifest.Dependencies;
		foreach (OwlcatModificationManifest.Dependency dependency in dependencies)
		{
			OwlcatModification owlcatModification = appliedModifications.FirstItem((OwlcatModification m) => m.Manifest?.UniqueName == dependency.Name);
			if (owlcatModification == null)
			{
				result = false;
				PFLog.Mods.Error("Dependency for " + uniqueName + " is missing: " + dependency.Name + " " + dependency.Version);
				break;
			}
			if (!dependency.Version.IsNullOrEmpty() && dependency.Version != owlcatModification.Manifest?.Version)
			{
				result = false;
				PFLog.Mods.Error("Wrong dependency version for " + uniqueName + ": wrong " + owlcatModification.Manifest?.Version + ", expected " + dependency.Version);
				break;
			}
		}
		return result;
	}

	[CanBeNull]
	public string GetBundleNameForAsset(string guid)
	{
		OwlcatModification[] appliedModifications = AppliedModifications;
		for (int i = 0; i < appliedModifications.Length; i++)
		{
			string bundleNameForAsset = appliedModifications[i].GetBundleNameForAsset(guid);
			if (bundleNameForAsset != null)
			{
				return bundleNameForAsset;
			}
		}
		return null;
	}

	[CanBeNull]
	public AssetBundle TryLoadBundle(string bundleName)
	{
		OwlcatModification[] appliedModifications = AppliedModifications;
		for (int i = 0; i < appliedModifications.Length; i++)
		{
			AssetBundle assetBundle = appliedModifications[i].TryLoadBundle(bundleName);
			if (assetBundle != null)
			{
				return assetBundle;
			}
		}
		return null;
	}

	[CanBeNull]
	public AssetBundleCreateRequest TryLoadBundleAsync(string bundleName)
	{
		OwlcatModification[] appliedModifications = AppliedModifications;
		for (int i = 0; i < appliedModifications.Length; i++)
		{
			AssetBundleCreateRequest assetBundleCreateRequest = appliedModifications[i].TryLoadBundleAsync(bundleName);
			if (assetBundleCreateRequest != null)
			{
				return assetBundleCreateRequest;
			}
		}
		return null;
	}

	public DependencyData GetDependenciesForBundle(string bundleName)
	{
		OwlcatModification[] appliedModifications = AppliedModifications;
		for (int i = 0; i < appliedModifications.Length; i++)
		{
			DependencyData dependenciesForBundle = appliedModifications[i].GetDependenciesForBundle(bundleName);
			if (dependenciesForBundle != null)
			{
				return dependenciesForBundle;
			}
		}
		return null;
	}

	public object OnResourceLoaded(object resource, string guid)
	{
		object obj = null;
		OwlcatModification[] appliedModifications = AppliedModifications;
		for (int i = 0; i < appliedModifications.Length; i++)
		{
			object resourceReplacement = appliedModifications[i].GetResourceReplacement(resource, guid);
			if (resourceReplacement is SimpleBlueprint)
			{
				resource = (obj = resourceReplacement);
			}
			else if (resourceReplacement != null)
			{
				PFLog.Mods.Error($"Replacing of resources {resource.GetType()} ({guid}) isn't supported");
			}
		}
		if (obj == null && resource is SimpleBlueprint simpleBlueprint)
		{
			bool flag = false;
			SimpleBlueprint simpleBlueprint2 = simpleBlueprint;
			appliedModifications = AppliedModifications;
			for (int i = 0; i < appliedModifications.Length; i++)
			{
				BlueprintPatch blueprintPatch = appliedModifications[i].GetBlueprintPatch(simpleBlueprint, guid);
				if (blueprintPatch != null)
				{
					flag = true;
					simpleBlueprint2 = BlueprintPatcher.TryPatchBlueprint(blueprintPatch, simpleBlueprint2, guid);
				}
			}
			if (flag)
			{
				resource = (obj = simpleBlueprint2);
			}
		}
		return obj;
	}

	[CanBeNull]
	public LocalizationPack LoadLocalizationPack(Locale locale)
	{
		LocalizationPack localizationPack = null;
		OwlcatModification[] appliedModifications = AppliedModifications;
		foreach (OwlcatModification owlcatModification in appliedModifications)
		{
			LocalizationPack localizationPack2 = owlcatModification.LoadLocalizationPack(locale) ?? owlcatModification.LoadLocalizationPack(Locale.enGB);
			if (localizationPack2 != null)
			{
				if (localizationPack == null)
				{
					localizationPack = new LocalizationPack
					{
						Locale = locale
					};
				}
				localizationPack.AddStrings(localizationPack2);
			}
		}
		return localizationPack;
	}

	public string GetLocalizedString(string key, Locale locale)
	{
		OwlcatModification[] appliedModifications = AppliedModifications;
		for (int i = 0; i < appliedModifications.Length; i++)
		{
			string localizedString = appliedModifications[i].GetLocalizedString(key, locale);
			if (!localizedString.IsNullOrEmpty())
			{
				return localizedString;
			}
		}
		return "";
	}

	[UsedImplicitly]
	[Cheat(Name = "reload_modifications_data", Description = "Reload data for all modifications", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatReloadData()
	{
		OwlcatModification[] appliedModifications = Instance.AppliedModifications;
		for (int i = 0; i < appliedModifications.Length; i++)
		{
			appliedModifications[i].Reload();
		}
		Instance.ApplyLocalization(Instance.m_LocalizationProvider.CurrentLocale);
	}
}
