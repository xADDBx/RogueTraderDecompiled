using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Code.GameCore.Blueprints.BlueprintPatcher;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.JsonSystem.Converters;
using Kingmaker.BundlesLoading;
using Kingmaker.Localization.Enums;
using Kingmaker.Localization.Shared;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using RogueTrader.SharedTypes;
using UnityEngine;

namespace Kingmaker.Modding;

public class OwlcatModification
{
	public delegate void LoadResourceCallback(object resource, string guid);

	public delegate void ShowGUICallback();

	public delegate void HideGUICallback();

	public delegate void DrawGUICallback();

	public delegate void SetEnabledCallback(bool enabled);

	public delegate bool IsEnabledDelegate();

	public const string ManifestFileName = "OwlcatModificationManifest.json";

	public const string SettingsFileName = "OwlcatModificationSettings.json";

	public const string BlueprintDirectReferencesBundleName = "BlueprintDirectReferences";

	public const string DataFilePostfix = "_Data.json";

	public static readonly Regex InvalidPathCharsRegex = new Regex("[" + Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars())) + "\\s]");

	public readonly string Path;

	public readonly string DataFilePath;

	public readonly OwlcatModificationManifest Manifest;

	[CanBeNull]
	public readonly Exception LoadException;

	[CanBeNull]
	private readonly ILocalizationProvider m_LocalizationProvider;

	public readonly HashSet<string> Bundles = new HashSet<string>();

	public readonly HashSet<string> Blueprints = new HashSet<string>();

	private readonly List<(Locale Locale, LocalizationPack Pack)> m_LocalizationPacks = new List<(Locale, LocalizationPack)>();

	[CanBeNull]
	private AssetBundle m_ReferencedAssetsBundle;

	[CanBeNull]
	private BlueprintReferencedAssets m_ReferencedAssets;

	[UsedImplicitly]
	[NotNull]
	public readonly LogChannel Logger;

	public OwlcatModificationSettings Settings { get; private set; }

	[CanBeNull]
	public Exception ApplyException { get; private set; }

	public Assembly[] LoadedAssemblies { get; private set; } = new Assembly[0];


	public string UniqueName => Manifest?.UniqueName;

	public bool Enabled { get; set; }

	[UsedImplicitly]
	[CanBeNull]
	public LoadResourceCallback OnLoadResource { get; set; }

	[UsedImplicitly]
	[CanBeNull]
	public ShowGUICallback OnShowGUI { get; set; }

	[UsedImplicitly]
	[CanBeNull]
	public HideGUICallback OnHideGUI { get; set; }

	[UsedImplicitly]
	[CanBeNull]
	public DrawGUICallback OnDrawGUI { get; set; }

	[UsedImplicitly]
	[CanBeNull]
	public SetEnabledCallback OnSetEnabled { get; set; }

	[UsedImplicitly]
	[CanBeNull]
	public IsEnabledDelegate IsEnabled { get; set; }

	[UsedImplicitly]
	[CanBeNull]
	public LoadResourceCallback LoadResourceCallbacks
	{
		get
		{
			return OnLoadResource;
		}
		set
		{
			OnLoadResource = value;
		}
	}

	[UsedImplicitly]
	[CanBeNull]
	public DrawGUICallback OnGUI
	{
		get
		{
			return OnDrawGUI;
		}
		set
		{
			OnDrawGUI = value;
		}
	}

	public static OwlcatModification LoadFromDirectory(string modificationDirectoryPath, string dataFolderPath, ILocalizationProvider localizationProvider)
	{
		try
		{
			return new OwlcatModification(modificationDirectoryPath, dataFolderPath, LoadJson<OwlcatModificationManifest>(modificationDirectoryPath, "OwlcatModificationManifest.json"), null, localizationProvider);
		}
		catch (Exception loadException)
		{
			return CreateInvalid(modificationDirectoryPath, loadException);
		}
	}

	private static OwlcatModification CreateInvalid(string path, Exception loadException)
	{
		return new OwlcatModification(path, null, null, loadException, null);
	}

	private OwlcatModification(string modificationPath, string dataFolderPath, [CanBeNull] OwlcatModificationManifest manifest, [CanBeNull] Exception loadException, [CanBeNull] ILocalizationProvider localizationProvider)
	{
		Path = modificationPath;
		Manifest = manifest;
		LoadException = loadException;
		m_LocalizationProvider = localizationProvider;
		if (Manifest != null)
		{
			string path = InvalidPathCharsRegex.Replace(Manifest.UniqueName, "") + "_Data.json";
			DataFilePath = System.IO.Path.Combine(dataFolderPath, path);
		}
		Logger = ((Manifest != null) ? LogChannelFactory.GetOrCreate(Manifest.UniqueName) : PFLog.Mods);
	}

	public void Apply()
	{
		try
		{
			ApplyInjections();
		}
		catch (Exception ex)
		{
			string text = "";
			if (Manifest != null)
			{
				text = Manifest.UniqueName + " : " + Manifest.DisplayName;
			}
			PFLog.Mods.Exception(ex, "Exception occured while applying modification " + text);
			ApplyException = ex;
		}
	}

	private void ApplyInjections()
	{
		if (Manifest != null)
		{
			Settings = LoadJson<OwlcatModificationSettings>(Path, "OwlcatModificationSettings.json");
			LoadAssemblies();
			Assembly[] loadedAssemblies = LoadedAssemblies;
			foreach (Assembly assembly in loadedAssemblies)
			{
				PFLog.Mods.Log("Initialize assembly: " + assembly.FullName);
				InvokeEnterPoint(assembly);
			}
		}
	}

	public void ApplyContent()
	{
		if (Manifest == null || Settings == null)
		{
			return;
		}
		try
		{
			LoadBundles();
			LoadBlueprints();
		}
		catch (Exception ex)
		{
			string text = "";
			if (Manifest != null)
			{
				text = Manifest.UniqueName + " : " + Manifest.DisplayName;
			}
			PFLog.Mods.Exception(ex, "Exception occured while applying modification " + text);
			ApplyException = ex;
		}
	}

	private void LoadAssemblies()
	{
		if (!LoadedAssemblies.Empty())
		{
			PFLog.Mods.Error("Can't load assemblies twice");
			return;
		}
		List<Assembly> list = new List<Assembly>();
		foreach (string item2 in GetFilesFromDirectory("Assemblies"))
		{
			PFLog.Mods.Log("Load assembly: " + item2);
			string text = item2.Replace('\\', '/');
			if (!text.EndsWith(".dll"))
			{
				if (!text.EndsWith(".pdb"))
				{
					PFLog.Mods.Log("Mod Assemblies folder contains non dll file. Please avoid it. " + text);
				}
			}
			else
			{
				Assembly item = Assembly.LoadFrom(GetModificationFilePath(text));
				list.Add(item);
			}
		}
		LoadedAssemblies = list.ToArray();
	}

	private void LoadBundles()
	{
		UnityObjectConverter.ModificationAssetLists.Remove(m_ReferencedAssets);
		ObjectExtensions.Or(m_ReferencedAssetsBundle, null)?.Unload(unloadAllLoadedObjects: true);
		m_ReferencedAssetsBundle = null;
		m_ReferencedAssets = null;
		Bundles.Clear();
		foreach (string item in GetFilesFromDirectory("Bundles"))
		{
			PFLog.Mods.Log("Bundle found: " + item);
			string fileName = System.IO.Path.GetFileName(item);
			Bundles.Add(fileName);
			if (fileName.EndsWith("BlueprintDirectReferences"))
			{
				m_ReferencedAssetsBundle = LoadBundle(fileName);
				m_ReferencedAssets = ObjectExtensions.Or(m_ReferencedAssetsBundle, null)?.LoadAllAssets<BlueprintReferencedAssets>().Single();
				UnityObjectConverter.ModificationAssetLists.Add(m_ReferencedAssets);
			}
		}
	}

	private void LoadBlueprints()
	{
		foreach (string blueprint in Blueprints)
		{
			ResourcesLibrary.BlueprintsCache.RemoveCachedBlueprint(blueprint);
		}
		Blueprints.Clear();
		foreach (string item in GetFilesFromDirectory("Blueprints"))
		{
			if (item.EndsWith(".jbp"))
			{
				PFLog.Mods.Log("Load blueprint: " + item);
				try
				{
					BlueprintJsonWrapper blueprintJsonWrapper = BlueprintJsonWrapper.Load(item);
					blueprintJsonWrapper.Data = TryPatchBlueprint(blueprintJsonWrapper.Data, blueprintJsonWrapper.AssetId) ?? blueprintJsonWrapper.Data;
					blueprintJsonWrapper.Data.OnEnable();
					ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(blueprintJsonWrapper.AssetId, blueprintJsonWrapper.Data);
					Blueprints.Add(blueprintJsonWrapper.AssetId);
				}
				catch (Exception ex)
				{
					PFLog.Mods.Exception(ex, "Failed loading blueprint: " + item);
				}
			}
		}
	}

	public void Reload()
	{
		PFLog.Mods.Log("Reload bundles and blueprints for modification: " + Path);
		string[] array = Bundles.ToArray();
		LoadBundles();
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (!text.EndsWith("BlueprintDirectReferences"))
			{
				BundlesLoadService.Instance.ReloadBundle(text);
			}
		}
		LoadBlueprints();
		m_LocalizationPacks.Clear();
		ApplyLocalization();
	}

	private static T LoadJson<T>(string path, string name)
	{
		string path2 = System.IO.Path.Combine(path, name);
		if (!File.Exists(path2))
		{
			throw new Exception(name + " file is missing");
		}
		return JsonUtility.FromJson<T>(File.ReadAllText(path2));
	}

	private IEnumerable<string> GetFilesFromDirectory(string directory)
	{
		return Directory.GetFiles(System.IO.Path.Combine(Path, directory), "*", SearchOption.AllDirectories);
	}

	private string GetModificationFilePath(string filename)
	{
		if (!System.IO.Path.IsPathRooted(filename))
		{
			return System.IO.Path.Combine(Path, filename);
		}
		return filename;
	}

	private string GetBlueprintPatchPath(string filename)
	{
		return GetModificationFilePath(System.IO.Path.Combine("Blueprints", filename));
	}

	private byte[] ReadAllBytes(string filename)
	{
		return File.ReadAllBytes(GetModificationFilePath(filename));
	}

	private void InvokeEnterPoint(Assembly assembly)
	{
		IEnumerable<MethodInfo> source = from t in assembly.GetTypes().SelectMany((Type type) => type.GetMethods())
			where t.HasAttribute<OwlcatModificationEnterPointAttribute>()
			select t;
		if (source.Empty())
		{
			PFLog.Mods.Log("Enter point not found: " + assembly.FullName);
			return;
		}
		if (source.Count() > 1)
		{
			throw new Exception("Multiple enter points in assembly: " + assembly.FullName);
		}
		MethodInfo methodInfo = source.First();
		if (!methodInfo.IsStatic)
		{
			throw new Exception("Assembly enter point must be static: " + assembly.FullName);
		}
		ParameterInfo[] parameters = methodInfo.GetParameters();
		if (parameters.Length > 1)
		{
			throw new Exception(string.Format(arg1: string.Join(", ", parameters.Select((ParameterInfo i) => i.ParameterType.Name)), format: "Assembly enter point has invalid parameters count ({0}): {1}", arg0: parameters.Length));
		}
		ParameterInfo parameterInfo = parameters.FirstOrDefault();
		if (parameterInfo != null && parameterInfo.ParameterType != typeof(OwlcatModification))
		{
			throw new Exception("Assembly enter point has invalid parameter type: " + parameterInfo.ParameterType.Name);
		}
		methodInfo.Invoke(null, (parameterInfo == null) ? new object[0] : new object[1] { this });
	}

	private static void PatchMaterialShaders(IEnumerable<Material> materials)
	{
		foreach (Material material in materials)
		{
			Shader shader = material.shader;
			if (shader != null)
			{
				Shader shader2 = Shader.Find(shader.name);
				if (shader2 == null)
				{
					PFLog.Mods.Error("Unable to find shader by name " + shader.name + " for material " + material.name);
					material.shader = shader;
				}
				else
				{
					material.shader = shader2;
				}
			}
			else
			{
				PFLog.Mods.Error("Unable to get shader name for material " + material.name);
			}
		}
	}

	private AssetBundle LoadBundle(string bundleName)
	{
		if (!Bundles.Contains(bundleName))
		{
			PFLog.Mods.Error("Modification '" + Manifest.UniqueName + "' doesn't contain bundle '" + bundleName + "'");
			return null;
		}
		AssetBundle assetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(Path, "Bundles/" + bundleName));
		if (!assetBundle.isStreamedSceneAssetBundle)
		{
			Material[] array = assetBundle.LoadAllAssets<OwlcatModificationMaterialsInBundleAsset>().SingleOrDefault()?.Materials;
			if (array != null)
			{
				PatchMaterialShaders(array);
			}
		}
		return assetBundle;
	}

	public AssetBundleCreateRequest LoadBundleAsync(string bundleName)
	{
		if (!Bundles.Contains(bundleName))
		{
			PFLog.Mods.Error("Modification '" + Manifest.UniqueName + "' doesn't contain bundle '" + bundleName + "'");
			return null;
		}
		return AssetBundle.LoadFromFileAsync(System.IO.Path.Combine(Path, "Bundles/" + bundleName));
	}

	[CanBeNull]
	public string GetBundleNameForAsset(string guid)
	{
		return Settings.BundlesLayout.GuidToBundle.Get(guid);
	}

	[CanBeNull]
	public AssetBundle TryLoadBundle(string bundleName)
	{
		if (!Settings.BundlesLayout.BundleNames.Contains(bundleName))
		{
			return null;
		}
		return LoadBundle(bundleName);
	}

	[CanBeNull]
	public AssetBundleCreateRequest TryLoadBundleAsync(string bundleName)
	{
		if (!Settings.BundlesLayout.BundleNames.Contains(bundleName))
		{
			return null;
		}
		return LoadBundleAsync(bundleName);
	}

	public DependencyData GetDependenciesForBundle(string bundleName)
	{
		if (!Settings.BundleDependencies.BundleToDependencies.ContainsKey(bundleName))
		{
			return null;
		}
		return Settings.BundleDependencies;
	}

	[UsedImplicitly]
	public T LoadData<T>() where T : new()
	{
		if (typeof(T).GetAttribute<SerializableAttribute>() == null)
		{
			throw new Exception("T must have [Serializable] attribute");
		}
		if (File.Exists(DataFilePath))
		{
			return JsonUtility.FromJson<T>(File.ReadAllText(DataFilePath));
		}
		return new T();
	}

	[UsedImplicitly]
	public void SaveData<T>(T data) where T : new()
	{
		if (typeof(T).GetAttribute<SerializableAttribute>() == null)
		{
			throw new Exception("T must have [Serializable] attribute");
		}
		File.WriteAllText(DataFilePath, JsonUtility.ToJson(data));
	}

	public BlueprintPatch GetBlueprintPatch(object resource, string guid)
	{
		BlueprintPatch blueprintPatch = null;
		if (!(resource is SimpleBlueprint simpleBlueprint))
		{
			return null;
		}
		if (guid == null)
		{
			return null;
		}
		string text = Settings.BlueprintPatchesDictionary.Get(guid);
		if (text == null)
		{
			return null;
		}
		try
		{
			PFLog.Mods.Log("Patching blueprint: " + simpleBlueprint.name + " (" + guid + ")");
			string text2 = GetBlueprintPatchPath(text) + ".jbp_patch";
			PFLog.Mods.Log("Patch path is: " + text2);
			if (!File.Exists(text2))
			{
				PFLog.Mods.Error("Patch file not found at path :" + text2);
				return null;
			}
			string text3 = File.ReadAllText(text2);
			using (StringReader reader = new StringReader(text3))
			{
				using JsonTextReader reader2 = new JsonTextReader(reader);
				blueprintPatch = Json.Serializer.Deserialize<BlueprintPatch>(reader2);
			}
			if (blueprintPatch == null)
			{
				PFLog.Mods.Error("Got null while deserializing at path " + text2 + " with contents: " + text3);
				return null;
			}
			PFLog.Mods.Log("Mods read patch: " + blueprintPatch.ToString());
			return blueprintPatch;
		}
		catch (Exception ex)
		{
			PFLog.Mods.Exception(ex, "Failed patch blueprint: " + simpleBlueprint.name + " (" + guid + ")");
			return null;
		}
	}

	public object GetResourceReplacement(object resource, string guid)
	{
		object obj = null;
		if (resource is SimpleBlueprint blueprint)
		{
			obj = TryPatchBlueprint(blueprint, guid);
			resource = obj ?? resource;
		}
		try
		{
			OnLoadResource?.Invoke(resource, guid);
		}
		catch (Exception ex)
		{
			PFLog.Mods.Exception(ex, "Failed invoke OnResourceLoaded");
		}
		return obj;
	}

	[CanBeNull]
	public LocalizationPack LoadLocalizationPack(Locale locale)
	{
		LocalizationPack localizationPack = m_LocalizationPacks.FirstItem(((Locale Locale, LocalizationPack Pack) i) => i.Locale == locale).Pack;
		if (localizationPack == null)
		{
			string packPath = System.IO.Path.Combine(Path, "Localization", locale.ToString() + ".json");
			if (m_LocalizationProvider != null)
			{
				localizationPack = m_LocalizationProvider.LoadPack(packPath, locale);
				m_LocalizationPacks.Add((locale, localizationPack));
			}
		}
		return localizationPack;
	}

	public void ApplyLocalization()
	{
		if (m_LocalizationProvider != null)
		{
			LocalizationPack localizationPack = LoadLocalizationPack(m_LocalizationProvider.CurrentLocale);
			if (localizationPack != null)
			{
				m_LocalizationProvider.CurrentPack?.AddStrings(localizationPack);
			}
		}
	}

	public string GetLocalizedString(string key, Locale locale)
	{
		return LoadLocalizationPack(locale)?.GetText(key, reportUnknown: false) ?? "";
	}

	[CanBeNull]
	private SimpleBlueprint TryPatchBlueprint(SimpleBlueprint blueprint, string guid)
	{
		if (guid == null)
		{
			return null;
		}
		string text = Settings.BlueprintReplacementsDictionary.Get(guid);
		if (text == null)
		{
			return null;
		}
		try
		{
			PFLog.Mods.Log("Patching blueprint: " + blueprint.name + " (" + guid + ")");
			string blueprintPatchPath = GetBlueprintPatchPath(text);
			return OwlcatModificationBlueprintPatcher.ApplyPatch(blueprint, blueprintPatchPath);
		}
		catch (Exception ex)
		{
			PFLog.Mods.Exception(ex, "Failed patch blueprint: " + blueprint.name + " (" + guid + ")");
			return null;
		}
	}
}
