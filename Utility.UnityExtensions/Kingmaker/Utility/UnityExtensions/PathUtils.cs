using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Kingmaker.Utility.UnityExtensions;

public static class PathUtils
{
	private const string BlueprintsFolder = "Blueprints";

	public static readonly string BundlesFolder = Path.Combine(Application.streamingAssetsPath, "Bundles");

	private static readonly HashSet<string> TextureExtensions = new HashSet<string> { ".tif", ".tga", ".png", ".jpg", ".psd", ".bmp", ".tiff", ".jpeg" };

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

	public static bool IsTextureFile(string path)
	{
		string text = Path.GetExtension(path)?.ToLowerInvariant();
		if (!string.IsNullOrEmpty(text))
		{
			return TextureExtensions.Contains(text);
		}
		return false;
	}
}
