using System.IO;
using UnityEngine;

namespace Kingmaker.Utility.UnityExtensions;

public static class PathUtils
{
	private const string BlueprintsFolder = "Blueprints";

	public static readonly string BundlesFolder = Path.Combine(Application.streamingAssetsPath, "Bundles");

	public static string BlueprintPath(string? pathComponent = null)
	{
		if (pathComponent == null)
		{
			return "Blueprints";
		}
		return Path.Combine("Blueprints", pathComponent);
	}

	public static string BundlePath(string? pathComponent = null)
	{
		if (pathComponent == null)
		{
			return BundlesFolder;
		}
		return Path.Combine(BundlesFolder, pathComponent);
	}
}
