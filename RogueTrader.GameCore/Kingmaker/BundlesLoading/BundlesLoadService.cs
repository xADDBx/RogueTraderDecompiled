using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase.ResourceReplacementProvider;
using Kingmaker.Modding;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility.Locator;
using UnityEngine;
using UnityEngine.Profiling;

namespace Kingmaker.BundlesLoading;

public class BundlesLoadService : IService
{
	private class BundleData
	{
		public AssetBundle Bundle;

		public int RequestCount;
	}

	private class AdditionalBundlesSourceData
	{
		public string Path;

		public HashSet<string> RequestedBundles = new HashSet<string>();
	}

	private static ServiceProxy<BundlesLoadService> s_Proxy;

	private DependencyData m_DependencyData;

	private readonly IResourceReplacementProvider m_ResourceReplacementProvider;

	private readonly Dictionary<string, BundleData> m_Bundles = new Dictionary<string, BundleData>();

	private LocationList m_LocationList;

	private readonly Dictionary<string, AdditionalBundlesSourceData> m_AdditionalBundlesSources = new Dictionary<string, AdditionalBundlesSourceData>();

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.Game;

	public static BundlesLoadService Instance
	{
		get
		{
			s_Proxy = ((s_Proxy?.Instance != null) ? s_Proxy : Services.GetProxy<BundlesLoadService>());
			return s_Proxy?.Instance;
		}
	}

	public BundlesLoadService(IResourceReplacementProvider resourceReplacementProvider)
	{
		m_ResourceReplacementProvider = resourceReplacementProvider;
	}

	public long GetTotalBundleMemory()
	{
		return (from d in m_Bundles
			select d.Value.Bundle into b
			where b
			select b).Sum((AssetBundle b) => Profiler.GetRuntimeMemorySizeLong(b));
	}

	public AssetBundle RequestBundleForAsset(string assetId)
	{
		string bundleNameForAsset = GetBundleNameForAsset(assetId);
		if (bundleNameForAsset == null)
		{
			PFLog.Bundles.Error("No bundle data for {0}", assetId);
			return null;
		}
		return RequestBundle(bundleNameForAsset);
	}

	public async Task<AssetBundle> RequestBundleForAssetAsync(string assetId)
	{
		string bundleNameForAsset = GetBundleNameForAsset(assetId);
		if (bundleNameForAsset == null)
		{
			PFLog.Bundles.Error("No bundle data for {0}", assetId);
			return null;
		}
		return await RequestBundleAsync(bundleNameForAsset);
	}

	public void UnloadBundleForAsset(string assetId)
	{
		string bundleNameForAsset = GetBundleNameForAsset(assetId);
		if (bundleNameForAsset == null)
		{
			PFLog.Bundles.Error("No bundle data for {0}", assetId);
		}
		else
		{
			UnloadBundle(bundleNameForAsset);
		}
	}

	public AssetBundle RequestBundle(string bundleName)
	{
		if (m_Bundles.TryGetValue(bundleName, out var value) && (bool)value.Bundle)
		{
			PFLog.Bundles.Log("Requested: {0}, already loaded ({1}) ", bundleName, value.RequestCount);
			value.RequestCount++;
			return value.Bundle;
		}
		value = (m_Bundles[bundleName] = new BundleData());
		PFLog.Bundles.Log("Requested: {0}, loading", bundleName);
		using (CodeTimer.New("LoadFromFile"))
		{
			value.Bundle = m_ResourceReplacementProvider?.TryLoadBundle(bundleName) ?? AssetBundle.LoadFromFile(BundlesPath(bundleName));
		}
		if ((bool)value.Bundle)
		{
			PFLog.Bundles.Log("Loaded bundle size: {0:### ### ###.00} kb", (float)Profiler.GetRuntimeMemorySizeLong(value.Bundle) / 1024f);
		}
		value.RequestCount++;
		LoadDependencies(bundleName);
		return value.Bundle;
	}

	public async Task<AssetBundle> RequestBundleAsync(string bundleName)
	{
		if (m_Bundles.TryGetValue(bundleName, out var data) && (bool)data.Bundle)
		{
			PFLog.Bundles.Log("Requested: {0}, already loaded ({1}) ", bundleName, data.RequestCount);
			data.RequestCount++;
			return data.Bundle;
		}
		Dictionary<string, BundleData> bundles = m_Bundles;
		BundleData value;
		data = (value = new BundleData());
		bundles[bundleName] = value;
		PFLog.Bundles.Log("Requested: {0}, loading", bundleName);
		AssetBundleCreateRequest request = OwlcatModificationsManager.Instance.TryLoadBundleAsync(bundleName) ?? AssetBundle.LoadFromFileAsync(BundlesPath(bundleName));
		BundleData bundleData = data;
		bundleData.Bundle = await WaitForBundleLoadOp(request);
		if ((bool)data.Bundle)
		{
			PFLog.Bundles.Log("Loaded bundle size: {0:### ### ###.00} kb", (float)Profiler.GetRuntimeMemorySizeLong(data.Bundle) / 1024f);
		}
		data.RequestCount++;
		await LoadDependenciesAsync(bundleName);
		return data.Bundle;
	}

	private static Task<AssetBundle> WaitForBundleLoadOp(AssetBundleCreateRequest request)
	{
		TaskCompletionSource<AssetBundle> tcs = new TaskCompletionSource<AssetBundle>();
		request.completed += delegate
		{
			tcs.SetResult(request.assetBundle);
		};
		return tcs.Task;
	}

	public void UnloadBundle(string bundleName)
	{
		if (!m_Bundles.TryGetValue(bundleName, out var value) || !value.Bundle)
		{
			return;
		}
		value.RequestCount--;
		if (value.RequestCount <= 0)
		{
			if (bundleName == "ui")
			{
				PFLog.Bundles.Error("Trying to unload common ui bundle: {0}, abort", bundleName);
				return;
			}
			PFLog.Bundles.Log("Unload: {0}, unloading", bundleName);
			value.Bundle.Unload(unloadAllLoadedObjects: true);
			value.Bundle = null;
			UnloadDependencies(bundleName);
		}
		else
		{
			PFLog.Bundles.Log("Unload: {0}, still needed ({1})", bundleName, value.RequestCount);
		}
	}

	[CanBeNull]
	private string GetBundleNameForAsset(string assetId)
	{
		return m_ResourceReplacementProvider?.GetBundleNameForAsset(assetId) ?? m_LocationList.GuidToBundle.Get(assetId);
	}

	private void LoadDependencies(string bundleName)
	{
		DependencyData dependencyData = m_ResourceReplacementProvider?.GetDependenciesForBundle(bundleName) ?? m_DependencyData;
		if (dependencyData == null || !dependencyData.BundleToDependencies.TryGetValue(bundleName, out var value))
		{
			return;
		}
		foreach (string item in value)
		{
			if (!AssetBundleNames.CommonBundles.HasItem(item))
			{
				RequestBundle(item);
			}
		}
	}

	private async Task LoadDependenciesAsync(string bundleName)
	{
		DependencyData dependencyData = m_ResourceReplacementProvider?.GetDependenciesForBundle(bundleName) ?? m_DependencyData;
		if (dependencyData == null || !dependencyData.BundleToDependencies.TryGetValue(bundleName, out var value))
		{
			return;
		}
		foreach (string item in value)
		{
			if (!AssetBundleNames.CommonBundles.HasItem(item))
			{
				await RequestBundleAsync(item);
			}
		}
	}

	private void UnloadDependencies(string bundleName)
	{
		DependencyData dependencyData = OwlcatModificationsManager.Instance.GetDependenciesForBundle(bundleName) ?? m_DependencyData;
		if (dependencyData == null || !dependencyData.BundleToDependencies.TryGetValue(bundleName, out var value))
		{
			return;
		}
		foreach (string item in value)
		{
			if (!AssetBundleNames.CommonBundles.HasItem(item))
			{
				UnloadBundle(item);
			}
		}
	}

	public IEnumerator RequestCommonBundlesCoroutine()
	{
		if (!ResourcesLibrary.UseBundles)
		{
			yield break;
		}
		ReadDependencyList();
		yield return null;
		ReadAssetLocationList();
		yield return null;
		for (int ii = 0; ii < AssetBundleNames.CommonBundles.Length; ii++)
		{
			string name = AssetBundleNames.CommonBundles[ii];
			AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(BundlesPath(name));
			while (!bundleRequest.isDone)
			{
				yield return null;
			}
			m_Bundles[name] = new BundleData
			{
				Bundle = bundleRequest.assetBundle,
				RequestCount = 1
			};
			yield return null;
		}
		using (CodeTimer.New("Common dependencies request"))
		{
			string[] commonBundles = AssetBundleNames.CommonBundles;
			foreach (string key in commonBundles)
			{
				if (!m_DependencyData.BundleToDependencies.TryGetValue(key, out var value))
				{
					continue;
				}
				foreach (string item in value)
				{
					if (!AssetBundleNames.CommonBundles.HasItem(item) && !m_Bundles.ContainsKey(item))
					{
						Task<AssetBundle> request = RequestBundleAsync(item);
						while (!request.IsCompleted)
						{
							yield return null;
						}
						yield return null;
					}
				}
			}
		}
	}

	private void ReadDependencyList()
	{
		using (CodeTimer.New("Load bundle dependencies"))
		{
			string path = BundlesPath("dependencylist.json");
			m_DependencyData = JsonConvert.DeserializeObject<DependencyData>(File.ReadAllText(path));
		}
	}

	private void ReadAssetLocationList()
	{
		using (CodeTimer.New("Load asset locations"))
		{
			string path = BundlesPath("locationlist.json");
			m_LocationList = JsonUtility.FromJson<LocationList>(File.ReadAllText(path));
		}
	}

	public static string BundlesBlueprintPath(string folderName)
	{
		return Path.Combine(Application.dataPath, "..", AssetBundleNames.GetBundlesFolder(), folderName);
	}

	public static string BundlesPath(string fileName)
	{
		string text = Path.Combine(Application.dataPath, "..", AssetBundleNames.GetBundlesFolder(), fileName);
		if (File.Exists(text))
		{
			return text;
		}
		PFLog.Bundles.Log("Bundle " + fileName + " not found at path: " + text);
		foreach (AdditionalBundlesSourceData value in Instance.m_AdditionalBundlesSources.Values)
		{
			text = Path.Combine(value.Path, AssetBundleNames.GetBundlesFolder(), fileName);
			if (File.Exists(text))
			{
				value.RequestedBundles.Add(fileName);
				return text;
			}
			PFLog.Bundles.Log("Bundle " + fileName + " not found at path: " + text);
		}
		PFLog.Bundles.Error("Bundle not found: " + fileName);
		return string.Empty;
	}

	public bool HasLocation(string assetId)
	{
		if (m_ResourceReplacementProvider?.GetBundleNameForAsset(assetId) == null)
		{
			return m_LocationList.GuidToBundle.ContainsKey(assetId);
		}
		return true;
	}

	public void ReloadBundle(string bundleName)
	{
		if (m_Bundles.TryGetValue(bundleName, out var value))
		{
			value.Bundle.Unload(unloadAllLoadedObjects: false);
			UnloadDependencies(bundleName);
			value.Bundle = m_ResourceReplacementProvider?.TryLoadBundle(bundleName) ?? AssetBundle.LoadFromFile(BundlesPath(bundleName));
			if (value.Bundle == null)
			{
				m_Bundles.Remove(bundleName);
			}
			else
			{
				LoadDependencies(bundleName);
			}
		}
	}

	public bool IsAssetBundleLoaded(string assetId)
	{
		if (m_Bundles.TryGetValue(GetBundleNameForAsset(assetId) ?? "", out var value))
		{
			return value.Bundle != null;
		}
		return false;
	}

	public void RegisterAdditionalBundlesSource(string bundlesSourceUniqueId, string bundlePath)
	{
		if (!bundlePath.IsNullOrEmpty())
		{
			m_AdditionalBundlesSources.Add(bundlesSourceUniqueId, new AdditionalBundlesSourceData
			{
				Path = bundlePath
			});
		}
	}

	public void UnregisterAdditionalBundlesSource(string bundlesSourceUniqueId)
	{
		if (bundlesSourceUniqueId.IsNullOrEmpty() || !m_AdditionalBundlesSources.TryGetValue(bundlesSourceUniqueId, out var value))
		{
			return;
		}
		foreach (string requestedBundle in value.RequestedBundles)
		{
			if (m_Bundles.TryGetValue(requestedBundle, out var value2))
			{
				PFLog.Bundles.Log("Force unload bundle: {0}", requestedBundle);
				value2.Bundle.Unload(unloadAllLoadedObjects: true);
				value2.Bundle = null;
				UnloadDependencies(requestedBundle);
			}
		}
		m_AdditionalBundlesSources.Remove(bundlesSourceUniqueId);
	}
}
