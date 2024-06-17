using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase.ResourceReplacementProvider;
using Kingmaker.BundlesLoading;
using Kingmaker.Utility.BuildModeUtils;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace Kingmaker.EntitySystem.Persistence.Scenes;

public static class BundledSceneLoader
{
	private static IResourceReplacementProvider s_ResourceReplacementProvider;

	public static void SetResourceReplacementProvider(IResourceReplacementProvider resourceReplacementProvider)
	{
		s_ResourceReplacementProvider = resourceReplacementProvider;
	}

	[NotNull]
	private static string GetBundleName(string sceneName)
	{
		return s_ResourceReplacementProvider?.GetBundleNameForAsset(sceneName) ?? (sceneName.ToLowerInvariant() + ".scenes");
	}

	public static void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Additive)
	{
		if (ResourcesLibrary.IsAssetInBundle(sceneName))
		{
			BundlesLoadService.Instance.RequestBundle(GetBundleName(sceneName));
		}
		SceneManagerWrapper.LoadScene(sceneName, mode);
	}

	public static async Task LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Additive)
	{
		long memStart = 0L;
		long memBundle = 0L;
		if (BuildModeUtility.Data?.Loading.LogSceneLoading ?? false)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			await Resources.UnloadUnusedAssets();
			memStart = Profiler.GetTotalAllocatedMemoryLong();
		}
		if (ResourcesLibrary.IsAssetInBundle(sceneName))
		{
			await BundlesLoadService.Instance.RequestBundleAsync(GetBundleName(sceneName));
		}
		if (BuildModeUtility.Data?.Loading.LogSceneLoading ?? false)
		{
			await Task.Yield();
			memBundle = Profiler.GetTotalAllocatedMemoryLong();
		}
		await (SceneManagerWrapper.LoadSceneAsync(sceneName, mode) ?? throw new InvalidOperationException("Cannot load scene " + sceneName));
		if (BuildModeUtility.Data?.Loading.LogSceneLoading ?? false)
		{
			await Task.Yield();
			long totalAllocatedMemoryLong = Profiler.GetTotalAllocatedMemoryLong();
			float num = (float)(memBundle - memStart) / 1024f / 1024f;
			float num2 = (float)(totalAllocatedMemoryLong - memBundle) / 1024f / 1024f;
			float num3 = (float)(totalAllocatedMemoryLong - memStart) / 1024f / 1024f;
			PFLog.SceneLoader.Log($"Loading scene: {sceneName} memory growth: {num:#.0} mb on bundles, {num2:#.0} mb on scene, {num3:#.0} mb total");
		}
	}

	public static async Task UnloadSceneAsync(string sceneName)
	{
		await SceneManager.UnloadSceneAsync(sceneName);
		if (ResourcesLibrary.IsAssetInBundle(sceneName))
		{
			BundlesLoadService.Instance.UnloadBundle(GetBundleName(sceneName));
		}
	}
}
