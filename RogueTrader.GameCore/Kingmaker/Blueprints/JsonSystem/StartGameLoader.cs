using System;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Converters;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase.ResourceReplacementProvider;
using Kingmaker.BundlesLoading;
using Kingmaker.Utility.CodeTimer;
using RogueTrader.SharedTypes;

namespace Kingmaker.Blueprints.JsonSystem;

public static class StartGameLoader
{
	public static void LoadDirectReferencesList()
	{
		using (CodeTimer.New("LoadDirectReferencesList"))
		{
			BlueprintReferencedAssets assetList = null;
			if (ResourcesLibrary.UseBundles)
			{
				assetList = (BlueprintReferencedAssets)BundlesLoadService.Instance.RequestBundle("blueprint.assets").LoadAllAssets(typeof(BlueprintReferencedAssets)).Single();
			}
			UnityObjectConverter.AssetList = assetList;
		}
	}

	public static void LoadPackTOC(IResourceReplacementProvider resourceReplacementProvider)
	{
		ResourcesLibrary.BlueprintsCache.Init(resourceReplacementProvider);
	}

	public static void LoadAllJson(string path)
	{
		if (!Directory.Exists(path))
		{
			return;
		}
		using (CodeTimer.New("LoadAllJson"))
		{
			foreach (string item in Directory.EnumerateFiles(path, "*.jbp", SearchOption.AllDirectories))
			{
				try
				{
					BlueprintJsonWrapper blueprintJsonWrapper = BlueprintJsonWrapper.Load(item);
					blueprintJsonWrapper.Data.OnEnable();
					ResourcesLibrary.BlueprintsCache.AddCachedBlueprint(blueprintJsonWrapper.AssetId, blueprintJsonWrapper.Data);
				}
				catch (Exception ex)
				{
					PFLog.Default.Error("Failed loading blueprint: " + item);
					PFLog.Default.Exception(ex);
				}
			}
		}
	}
}
