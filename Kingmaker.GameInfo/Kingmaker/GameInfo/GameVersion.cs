using System.IO;
using Kingmaker.Utility.BuildModeUtils;
using UnityEngine;

namespace Kingmaker.GameInfo;

public class GameVersion : ScriptableObject
{
	public string Version;

	private static string s_Cached;

	private const string RevisionFileRelativePath = "Version.info";

	private static string s_Revision;

	private static string s_DeviceIdentifier;

	public static BuildMode Mode => BuildModeUtility.Mode;

	public static string Revision => s_Revision ?? (s_Revision = GetRevision());

	public static string DeviceUniqueIdentifier => s_DeviceIdentifier ?? (s_DeviceIdentifier = SystemInfo.deviceUniqueIdentifier);

	public static string GetVersion()
	{
		if (s_Cached != null)
		{
			return s_Cached;
		}
		GameVersion gameVersion = Resources.Load<GameVersion>("Version");
		return s_Cached = ((gameVersion == null) ? "Editor" : gameVersion.Version);
	}

	public static string GetDisplayedVersion()
	{
		return BuildModeUtility.OverrideDisplayedGameVersion;
	}

	private static string GetRevision()
	{
		s_DeviceIdentifier = SystemInfo.deviceUniqueIdentifier;
		string path = Path.Combine(Application.streamingAssetsPath, "Version.info");
		if (!File.Exists(path))
		{
			return "Editor";
		}
		return File.ReadAllText(path);
	}
}
