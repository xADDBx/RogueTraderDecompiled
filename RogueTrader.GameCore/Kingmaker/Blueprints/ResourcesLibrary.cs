using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase.ResourceReplacementProvider;
using Kingmaker.BundlesLoading;
using Kingmaker.ElementsSystem;
using Kingmaker.ResourceLinks;
using Kingmaker.ResourceLinks.BaseInterfaces;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Particles.GameObjectsPooling;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using Pathfinding.Util;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.Blueprints;

public static class ResourcesLibrary
{
	public enum CleanupMode
	{
		UnloadEverything,
		UnloadAndCleanRequests,
		UnloadNonRequested
	}

	private class LoadedResource
	{
		[CanBeNull]
		public UnityEngine.Object Resource;

		public string AssetId;

		public int RequestCounter;

		public int HandleCounter;

		public LoadedResource([NotNull] UnityEngine.Object resource)
		{
			Resource = resource;
		}

		public LoadedResource()
		{
		}

		public void Unload()
		{
			if (!string.IsNullOrEmpty(AssetId))
			{
				BundlesLoadService.Instance.UnloadBundleForAsset(AssetId);
			}
		}
	}

	private static bool s_Initialized;

	private static bool s_Preloading;

	private static readonly Queue<Func<UnityEngine.Object>> s_PreloadActionsQueue = new Queue<Func<UnityEngine.Object>>();

	private static bool s_SynchronousPreloading;

	private static bool s_DisablePreloading;

	private static IResourceReplacementProvider s_ResourceReplacementProvider;

	[NotNull]
	private static readonly Dictionary<string, LoadedResource> s_LoadedResources = new Dictionary<string, LoadedResource>();

	public static readonly BlueprintsCache BlueprintsCache = new BlueprintsCache();

	private static AssetBundle s_BlueprintsBundle;

	public static object LoadedResources => s_LoadedResources;

	public static bool Preloading => s_Preloading;

	public static bool UseBundles => BundlesUsageProvider.UseBundles;

	public static void InitializeLibrary(IResourceReplacementProvider resourceReplacementProvider)
	{
		s_ResourceReplacementProvider = resourceReplacementProvider;
		if (UseBundles)
		{
			s_BlueprintsBundle = BundlesLoadService.Instance.RequestBundle("blueprint.assets");
		}
	}

	public static bool IsAssetInBundle(string assetGuid)
	{
		if (!UseBundles)
		{
			return s_ResourceReplacementProvider?.GetBundleNameForAsset(assetGuid) != null;
		}
		return true;
	}

	[CanBeNull]
	public static SimpleBlueprint TryGetBlueprint(string assetId)
	{
		if (string.IsNullOrEmpty(assetId))
		{
			return null;
		}
		return BlueprintsCache.Load(assetId);
	}

	[CanBeNull]
	public static ElementsScriptableObject TryGetScriptable(string assetId)
	{
		if (string.IsNullOrEmpty(assetId))
		{
			return null;
		}
		return TryGetBlueprint(assetId) as ElementsScriptableObject;
	}

	public static void StartPreloadingMode()
	{
		s_Preloading = true;
	}

	public static void StopPreloadingMode()
	{
		if (s_PreloadActionsQueue.Count > 0)
		{
			PFLog.Default.Error("Stopping preloading while there are still preload requests pending");
			s_PreloadActionsQueue.Clear();
		}
		s_Preloading = false;
	}

	public static bool TickPreloading()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		while (stopwatch.ElapsedMilliseconds < 400 && s_PreloadActionsQueue.Count > 0)
		{
			UnityEngine.Object @object = s_PreloadActionsQueue.Dequeue()();
			try
			{
				(@object as IRecursivePreloadResource)?.DoRecursivePreload();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
		stopwatch.Stop();
		return s_PreloadActionsQueue.Count > 0;
	}

	public static void PreloadResource<TResource>(string assetId) where TResource : UnityEngine.Object
	{
		if (!s_Preloading)
		{
			PFLog.Default.Error("Resources preloading is only allowed in preloading mode");
		}
		else
		{
			if (s_DisablePreloading || string.IsNullOrEmpty(assetId))
			{
				return;
			}
			if (IsAssetInBundle(assetId) && !BundlesLoadService.Instance.HasLocation(assetId))
			{
				PFLog.Default.Error("Resources preloading: {0} has no bundle location", assetId);
				return;
			}
			LoadedResource loadedResource = s_LoadedResources.Get(assetId);
			if (loadedResource != null)
			{
				loadedResource.RequestCounter++;
				return;
			}
			loadedResource = new LoadedResource();
			loadedResource.RequestCounter++;
			s_LoadedResources[assetId] = loadedResource;
			if (s_SynchronousPreloading)
			{
				LoadResource<TResource>(assetId, loadedResource);
			}
			else
			{
				StartPreload<TResource>(assetId, loadedResource);
			}
		}
	}

	public static bool IsLoaded(string assetId)
	{
		return s_LoadedResources.ContainsKey(assetId);
	}

	public static IEnumerable<TResource> GetLoadedResourcesOfType<TResource>() where TResource : UnityEngine.Object
	{
		foreach (string key in s_LoadedResources.Keys)
		{
			if (s_LoadedResources[key].Resource is TResource val)
			{
				yield return val;
			}
		}
	}

	public static IEnumerable<string> GetLoadedAssetIdsOfType<TResource>() where TResource : UnityEngine.Object
	{
		foreach (string key in s_LoadedResources.Keys)
		{
			if (s_LoadedResources[key].Resource is TResource)
			{
				yield return key;
			}
		}
	}

	[CanBeNull]
	public static TResource TryGetResource<TResource>(string assetId, bool ignorePreloadWarning = false, bool hold = false) where TResource : UnityEngine.Object
	{
		if (s_Preloading && !ignorePreloadWarning)
		{
			PFLog.Default.Error("Resources loading is forbidden in preloading mode");
			return null;
		}
		LoadedResource loadedResource = s_LoadedResources.Get(assetId);
		if (loadedResource == null)
		{
			try
			{
				loadedResource = new LoadedResource();
				LoadResource<TResource>(assetId, loadedResource);
				PFLog.Default.Log($"Loaded non-preloaded resource {assetId} ({loadedResource.Resource})");
				s_LoadedResources[assetId] = loadedResource;
			}
			finally
			{
			}
		}
		loadedResource.RequestCounter++;
		if (hold)
		{
			loadedResource.HandleCounter++;
		}
		if (typeof(TResource) == typeof(GameObject) && (bool)loadedResource.Resource)
		{
			MonoBehaviour monoBehaviour = loadedResource.Resource as MonoBehaviour;
			if (monoBehaviour != null)
			{
				return monoBehaviour.gameObject as TResource;
			}
		}
		return loadedResource.Resource as TResource;
	}

	[CanBeNull]
	public static async Task<TResource> TryGetResourceAsync<TResource>(string assetId, bool ignorePreloadWarning = false, bool hold = false) where TResource : UnityEngine.Object
	{
		if (s_Preloading && !ignorePreloadWarning)
		{
			PFLog.Default.Error("Resources loading is forbidden in preloading mode");
			return null;
		}
		LoadedResource loaded = s_LoadedResources.Get(assetId);
		if (loaded == null)
		{
			try
			{
				loaded = new LoadedResource();
				await LoadResourceAsync<TResource>(assetId, loaded);
				PFLog.Default.Log($"Loaded non-preloaded resource {assetId} ({loaded.Resource})");
				s_LoadedResources[assetId] = loaded;
			}
			finally
			{
				_ = 0;
			}
		}
		loaded.RequestCounter++;
		if (hold)
		{
			loaded.HandleCounter++;
		}
		if (typeof(TResource) == typeof(GameObject) && (bool)loaded.Resource)
		{
			MonoBehaviour monoBehaviour = loaded.Resource as MonoBehaviour;
			if (monoBehaviour != null)
			{
				return monoBehaviour.gameObject as TResource;
			}
		}
		return loaded.Resource as TResource;
	}

	public static void FreeResourceRequest(string assetId, bool held = false)
	{
		LoadedResource loadedResource = s_LoadedResources.Get(assetId);
		if (loadedResource != null)
		{
			loadedResource.RequestCounter--;
			if (held)
			{
				loadedResource.HandleCounter--;
			}
		}
	}

	public static bool HasLoadedResource(string assetId)
	{
		return s_LoadedResources.Get(assetId) != null;
	}

	public static void ForceUnloadResource(string assetId)
	{
		LoadedResource loadedResource = s_LoadedResources.Get(assetId);
		s_LoadedResources.Remove(assetId);
		if (loadedResource == null)
		{
			return;
		}
		UnityEngine.Object resource = loadedResource.Resource;
		loadedResource.Unload();
		if (!UseBundles)
		{
			return;
		}
		if (BundlesLoadService.Instance.IsAssetBundleLoaded(assetId))
		{
			PFLog.Bundles.Warning("Force-Unloaded resource " + assetId + " but its bundle is still loaded");
		}
		else
		{
			if (!(resource != null))
			{
				return;
			}
			if (!(resource is GameObject obj))
			{
				if (!(resource is Component component))
				{
					if ((object)resource != null)
					{
						Resources.UnloadAsset(resource);
					}
				}
				else if (component != null)
				{
					UnityEngine.Object.Destroy(component.gameObject);
				}
			}
			else
			{
				UnityEngine.Object.Destroy(obj);
			}
		}
	}

	public static void CleanupLoadedCache(CleanupMode mode = CleanupMode.UnloadAndCleanRequests)
	{
		if (s_Preloading)
		{
			PFLog.Default.Error("Cannot Cleanup cache in preloading mode");
			return;
		}
		List<string> list = Pathfinding.Util.ListPool<string>.Claim();
		try
		{
			foreach (KeyValuePair<string, LoadedResource> s_LoadedResource in s_LoadedResources)
			{
				if (!IsPooledResource(s_LoadedResource.Value.Resource))
				{
					if (s_LoadedResource.Value.RequestCounter <= 0 || mode == CleanupMode.UnloadEverything)
					{
						s_LoadedResource.Value.Unload();
						list.Add(s_LoadedResource.Key);
					}
					if (mode == CleanupMode.UnloadAndCleanRequests && s_LoadedResource.Value.HandleCounter <= 0)
					{
						s_LoadedResource.Value.RequestCounter = 0;
					}
				}
			}
			foreach (string item in list)
			{
				s_LoadedResources.Remove(item);
			}
		}
		finally
		{
			Pathfinding.Util.ListPool<string>.Release(list);
		}
	}

	private static bool IsPooledResource(UnityEngine.Object resource)
	{
		GameObject gameObject = resource as GameObject;
		if (gameObject == null)
		{
			MonoBehaviour monoBehaviour = resource as MonoBehaviour;
			if (monoBehaviour != null)
			{
				gameObject = monoBehaviour.gameObject;
			}
		}
		if (gameObject == null)
		{
			return false;
		}
		PooledGameObject component = gameObject.GetComponent<PooledGameObject>();
		if (component != null)
		{
			return GameObjectsPool.HasPoolInstances(component);
		}
		return false;
	}

	[CanBeNull]
	private static void LoadResource<TResource>(string assetId, LoadedResource loaded) where TResource : UnityEngine.Object
	{
		try
		{
			if (string.IsNullOrEmpty(assetId))
			{
				return;
			}
			bool isPC = true;
			if (IsAssetInBundle(assetId))
			{
				TResource val = null;
				AssetBundle assetBundle = BundlesLoadService.Instance.RequestBundleForAsset(assetId);
				if ((bool)assetBundle)
				{
					loaded.AssetId = assetId;
					using (AddContextToUnityLog.New(assetId))
					{
						GameObject gameObject = null;
						if (typeof(TResource).IsSubclassOf(typeof(MonoBehaviour)))
						{
							gameObject = assetBundle.LoadAsset(assetId) as GameObject;
							if (gameObject != null)
							{
								val = gameObject.GetComponent<TResource>();
							}
						}
						else
						{
							val = assetBundle.LoadAsset<TResource>(assetId);
						}
						gameObject = ObjectExtensions.Or(gameObject, null) ?? (val as GameObject);
						TryToModifyResourceContent(gameObject, isPC);
					}
				}
				if (val == null)
				{
					PFLog.Default.Warning("Could not load resource (id = " + assetId + ")");
					loaded.Unload();
				}
				else
				{
					StoreResource<TResource>(loaded, val);
				}
				return;
			}
			throw new InvalidOperationException("LibraryObject does not exist");
		}
		finally
		{
		}
	}

	private static async Task LoadResourceAsync<TResource>(string assetId, LoadedResource loaded) where TResource : UnityEngine.Object
	{
		_ = 2;
		try
		{
			if (string.IsNullOrEmpty(assetId))
			{
				return;
			}
			bool isPC = true;
			if (IsAssetInBundle(assetId))
			{
				TResource resource = null;
				AssetBundle assetBundle = await BundlesLoadService.Instance.RequestBundleForAssetAsync(assetId);
				if ((bool)assetBundle)
				{
					loaded.AssetId = assetId;
					using (AddContextToUnityLog.New(assetId))
					{
						GameObject go = null;
						if (typeof(TResource).IsSubclassOf(typeof(MonoBehaviour)))
						{
							go = (await LoadAssetAsync<UnityEngine.Object>(assetBundle, assetId)) as GameObject;
							if (go != null)
							{
								resource = go.GetComponent<TResource>();
							}
						}
						else
						{
							resource = await LoadAssetAsync<TResource>(assetBundle, assetId);
						}
						go = ObjectExtensions.Or(go, null) ?? (resource as GameObject);
						TryToModifyResourceContent(go, isPC);
					}
				}
				if (resource == null)
				{
					PFLog.Default.Warning("Could not load resource (id = " + assetId + ")");
					loaded.Unload();
				}
				else
				{
					StoreResource<TResource>(loaded, resource);
				}
				return;
			}
			throw new InvalidOperationException("LibraryObject does not exist");
		}
		finally
		{
			_ = 0;
		}
	}

	private static Task<TResource> LoadAssetAsync<TResource>(AssetBundle bundle, string assetId) where TResource : UnityEngine.Object
	{
		TaskCompletionSource<TResource> tcs = new TaskCompletionSource<TResource>();
		AssetBundleRequest request = bundle.LoadAssetAsync<TResource>(assetId);
		request.completed += delegate
		{
			tcs.SetResult((TResource)request.asset);
		};
		return tcs.Task;
	}

	private static void TryToModifyResourceContent(GameObject go, bool isPC)
	{
		if (!(go != null))
		{
			return;
		}
		using (ProfileScope.New("RemoveObjectOnPlatform"))
		{
			foreach (RemoveObjectOnPlatform item in go.EnumerateComponentsInChildren<RemoveObjectOnPlatform>(includeInactive: true))
			{
				if ((isPC && item.RemoveOnPC) || (!isPC && item.RemoveOnConsole))
				{
					UnityEngine.Object.DestroyImmediate(item.gameObject, allowDestroyingAssets: true);
				}
			}
		}
		using (ProfileScope.New("DisableShadowsOnDynamicObjects"))
		{
			if (!BuildModeUtility.Data.DisableShadowsOnDynamicObjects)
			{
				return;
			}
			foreach (Renderer item2 in go.EnumerateComponentsInChildren<Renderer>(includeInactive: true))
			{
				item2.shadowCastingMode = ShadowCastingMode.Off;
			}
		}
	}

	private static void StartPreload<TResource>(string assetId, LoadedResource loaded) where TResource : UnityEngine.Object
	{
		if (Application.isPlaying)
		{
			s_PreloadActionsQueue.Enqueue(delegate
			{
				LoadResource<TResource>(assetId, loaded);
				return s_LoadedResources.Get(assetId)?.Resource;
			});
		}
	}

	private static void StoreResource<TResource>(LoadedResource loaded, UnityEngine.Object resource) where TResource : UnityEngine.Object
	{
		if (typeof(TResource) == typeof(GameObject))
		{
			GameObject gameObject = resource as GameObject;
			if (gameObject != null)
			{
				MonoBehaviour monoBehaviour = gameObject.GetComponent<IResource>() as MonoBehaviour;
				if (monoBehaviour != null)
				{
					loaded.Resource = monoBehaviour;
				}
				else
				{
					loaded.Resource = gameObject;
				}
			}
		}
		else
		{
			loaded.Resource = resource as TResource;
		}
		string assetId = loaded.AssetId;
		s_ResourceReplacementProvider?.OnResourceLoaded(resource, assetId);
	}

	[CanBeNull]
	public static TBlueprint TryGetBlueprint<TBlueprint>(string assetId) where TBlueprint : BlueprintScriptableObject
	{
		return (TBlueprint)TryGetBlueprint(assetId);
	}

	[CanBeNull]
	public static T TryGetScriptable<T>(string assetId) where T : ElementsScriptableObject
	{
		return (T)TryGetScriptable(assetId);
	}
}
