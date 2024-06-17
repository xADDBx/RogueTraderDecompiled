using UnityEngine;
using UnityEngine.Device;

namespace Kingmaker.BundlesLoading;

public class AssetBundleNames
{
	public const string TechArt = "techart";

	public const string FX = "fx";

	public const string SceneSuffix = ".scenes";

	public const string TerrainsSuffix = ".terrainlayers";

	public const string WorldPrefabsSuffix = ".worldart";

	public const string WorldTexturesSuffix = ".worldtex";

	public const string AnimationSuffix = ".animations";

	public const string Equipment = "equipment";

	public const string Icons = "icons";

	public const string UIArt = "ui";

	public const string Portraits = "portraits";

	public const string UnitSuffix = ".unit";

	public const string Weapons = "weapons.prefabs";

	public const string Encyclopedia = "encyclopedia";

	public const string Tactical = "tactical.prefabs";

	public const string Kingdom = "buildings.prefabs";

	public const string MapObjects = "mapobjects.prefabs";

	public const string BlueprintAssets = "blueprint.assets";

	public const string HumanAnimation = "humananimation.animations";

	public const string Shaders = "shaders";

	public const string SpaceArt = "spaceart";

	public const string Extra = "extra";

	public const string ViewResSuffix = ".res";

	public static readonly string[] CommonBundles = new string[11]
	{
		"shaders", "techart", "fx", "ui", "icons", "portraits", "encyclopedia", "equipment", "weapons.prefabs", "blueprint.assets",
		"humananimation.animations"
	};

	public const string BundleFolderPC = "Bundles";

	public const string BundleFolderPS4 = "Media/Bundles";

	public const string BundleFolderPS5 = "Media/Bundles";

	public const string BundleFolderXBoxOne = "Bundles";

	public const string BundleFolderXBoxSeries = "Bundles";

	public const string BundleFolderOsx = "Bundles";

	public const string BundleFolderEditor = "IntermediateBuild/Bundles";

	public static string GetBundlesFolder()
	{
		return UnityEngine.Device.Application.platform switch
		{
			RuntimePlatform.PS4 => "Media/Bundles", 
			RuntimePlatform.PS5 => "Media/Bundles", 
			RuntimePlatform.XboxOne => "Bundles", 
			RuntimePlatform.GameCoreXboxSeries => "Bundles", 
			RuntimePlatform.OSXPlayer => "Bundles", 
			RuntimePlatform.WindowsPlayer => "Bundles", 
			_ => "IntermediateBuild/Bundles", 
		};
	}
}
