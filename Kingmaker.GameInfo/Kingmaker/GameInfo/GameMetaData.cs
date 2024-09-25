using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.GameInfo;

public class GameMetaData
{
	private static string s_VersionCached;

	private static string s_BuildGuid;

	[JsonProperty]
	public string Version { get; set; }

	[JsonProperty]
	public string BuildGuid { get; set; }

	[JsonProperty]
	public CheckSumInfo[] DllCRC { get; set; }

	[JsonProperty]
	public bool Mod { get; set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void Init()
	{
		s_VersionCached = GameVersion.GetVersion();
		s_BuildGuid = Application.buildGUID;
	}

	public static GameMetaData Create(CheckSumInfo[] DllCrc, bool isUnityModManagerActive)
	{
		return new GameMetaData
		{
			Version = s_VersionCached,
			BuildGuid = s_BuildGuid,
			Mod = isUnityModManagerActive,
			DllCRC = DllCrc
		};
	}
}
