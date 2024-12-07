using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.Utility.BuildModeUtils;

public class BuildModeUtility
{
	private static readonly string s_JsonName = "startup.json";

	private static readonly string s_JsonPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));

	private static readonly string s_JsonFile = Path.Combine(s_JsonPath, s_JsonName);

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
					return !Data.betatest;
				}
				return false;
			}
			return true;
		}
	}

	public static bool IsBeta => Data?.betatest ?? false;

	public static BuildMode Mode
	{
		get
		{
			if (!IsRelease)
			{
				if (!IsBeta)
				{
					return BuildMode.Development;
				}
				return BuildMode.Betatest;
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

	public static void CreateJsonMode(string folderPath, StartupJson json)
	{
		using StreamWriter streamWriter = new StreamWriter(Path.Combine(folderPath, s_JsonName));
		streamWriter.WriteLine(JsonConvert.SerializeObject(json, Formatting.Indented));
		streamWriter.Close();
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

	private static StartupJson GetStartUpData()
	{
		if (s_Data != null)
		{
			return s_Data;
		}
		string[] array = new string[2]
		{
			s_JsonFile,
			Path.Combine(Application.streamingAssetsPath, s_JsonName)
		};
		foreach (string text in array)
		{
			if (File.Exists(text))
			{
				using (StreamReader streamReader = new StreamReader(text))
				{
					s_Data = JsonConvert.DeserializeObject<StartupJson>(streamReader.ReadToEnd());
				}
				ActualPathToStartUp = text;
				break;
			}
		}
		bool flag = Application.isEditor || Debug.isDebugBuild;
		if (s_Data == null)
		{
			s_Data = new StartupJson
			{
				development = flag,
				EnableCheats = flag,
				EnableCheatStore = flag
			};
		}
		return s_Data;
	}
}
