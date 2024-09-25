using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Networking.Desync;

public class ReleasePropsCollector : IPropsCollector
{
	public const string GameVersionKey = "game_version";

	public const string LoadedAreaKey = "loaded_area";

	public const string LoadedAreaPartKey = "loaded_area_part";

	public Dictionary<string, string> Collect()
	{
		return new Dictionary<string, string>
		{
			["game_version"] = Application.version,
			["loaded_area"] = Game.Instance.CurrentlyLoadedArea.AssetGuid,
			["loaded_area_part"] = Game.Instance.CurrentlyLoadedAreaPart.AssetGuid
		};
	}
}
