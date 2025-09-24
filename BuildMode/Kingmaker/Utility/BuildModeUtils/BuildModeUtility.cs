using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.Utility.BuildModeUtils;

public class BuildModeUtility
{
	public const string s_JsonName = "startup.json";

	private static readonly string s_JsonPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));

	private static readonly string s_JsonFile = Path.Combine(s_JsonPath, "startup.json");

	private static StartupJson s_Data;

	public static string ActualPathToStartUp { get; private set; }

	public static string Address
	{
		get
		{
			if (Data != null && !string.IsNullOrEmpty(Data?.address))
			{
				return Data.address;
			}
			return string.Empty;
		}
	}

	public static bool IsDevelopment => Data?.development ?? false;

	public static bool CheatsEnabled => Data?.EnableCheats ?? IsDevelopment;

	public static bool CheatStoreEnabled => Data?.EnableCheatStore ?? IsDevelopment;

	public static bool IsShowDevComment => Data?.showDevComment ?? false;

	public static bool StackTraceSpamDetectionDisabled => Data?.stackTraceSpamDetectionDisabled ?? false;

	public static bool ForceSpamDetectionInEditor => Data?.forceSpamDetectionInEditor ?? false;

	public static string AnalyticsUrl => Data?.analyticsUrl;

	public static bool ForceAnalyticsInEditor => Data?.forceAnalyticsInEditor ?? false;

	public static string LogsDir => Data?.logsDir;

	public static bool IsLogExtended => Data?.logsExtended ?? false;

	public static bool IsRelease
	{
		get
		{
			if (Data != null)
			{
				if (Data != null && !Data.development)
				{
					return !Data.playtest;
				}
				return false;
			}
			return true;
		}
	}

	public static bool IsPlayTest => Data?.playtest ?? false;

	public static BuildMode Mode
	{
		get
		{
			if (!IsRelease)
			{
				if (!IsPlayTest)
				{
					return BuildMode.Development;
				}
				return BuildMode.Playtest;
			}
			return BuildMode.Release;
		}
	}

	public static string OverrideDisplayedGameVersion => Data?.OverrideDisplayedGameVersion;

	public static bool EnableTextureQualityLoweringToReduceMemoryUsage => Data?.EnableTextureQualityLoweringToReduceMemoryUsage ?? false;

	public static bool ExtraOptimization => Data.EnableExtraOptimizations;

	public static bool IncludeAllDesyncs => Data?.IncludeAllDesyncs ?? false;

	public static bool HoldToSkipCutscene => Data?.HoldToSkipCutscene ?? false;

	public static int ClientInstanceId => Data?.ClientInstanceId ?? 0;

	public static bool ReplaySaveState => Data?.ReplaySaveState ?? false;

	public static StartupJson Data => GetStartUpData();

	private static bool IsLowMemoryPlatform
	{
		get
		{
			if (Application.isEditor)
			{
				return false;
			}
			return SystemInfo.systemMemorySize <= 8192;
		}
	}

	public static bool CheckAndEnableLowMemoryOptimizations()
	{
		if (!IsLowMemoryPlatform || (Data?.Loading?.DisableLowMemoryOverrides).GetValueOrDefault())
		{
			return false;
		}
		Data.DisablePreload = true;
		StartupJson data = Data;
		if (data.Loading == null)
		{
			data.Loading = new StartupJson.LoadingResearchSettings();
		}
		Data.Loading.DestroyUIPrefabs = true;
		Data.Loading.UnloadSceneBundles = true;
		Data.Loading.WidgetStashCleanup = true;
		Data.ForceGCOnSaves = true;
		Data.EnableTextureQualityLoweringToReduceMemoryUsage = true;
		return true;
	}

	[NotNull]
	private static StartupJson GetStartUpData()
	{
		if (s_Data != null)
		{
			return s_Data;
		}
		string[] array = new string[2]
		{
			s_JsonFile,
			Path.Combine(Application.streamingAssetsPath, "startup.json")
		};
		foreach (string text in array)
		{
			if (File.Exists(text))
			{
				Debug.LogFormat("Found {0} at {1}", "startup.json", text);
				using (StreamReader streamReader = new StreamReader(text))
				{
					s_Data = JsonConvert.DeserializeObject<StartupJson>(streamReader.ReadToEnd());
				}
				ActualPathToStartUp = text;
				return s_Data;
			}
		}
		Debug.LogFormat("{0} not found, specifically at {1}", "startup.json", s_JsonFile);
		bool flag = Application.isEditor || Debug.isDebugBuild;
		s_Data = new StartupJson
		{
			development = flag,
			EnableCheats = flag,
			EnableCheatStore = flag
		};
		return s_Data;
	}
}
