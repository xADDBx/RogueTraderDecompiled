using UnityEngine;

namespace Kingmaker.Utility;

public static class VideoHelper
{
	public static readonly Resolution FHDResolution = new Resolution
	{
		width = 1920,
		height = 1080
	};

	public static bool IsCurrentResolutionMoreThanFHD => IsCurrentResolutionMoreThan(FHDResolution);

	public static bool IsCurrentResolutionMoreThan(Resolution resolution)
	{
		if (Screen.currentResolution.width > resolution.width)
		{
			return Screen.currentResolution.height > resolution.height;
		}
		return false;
	}
}
