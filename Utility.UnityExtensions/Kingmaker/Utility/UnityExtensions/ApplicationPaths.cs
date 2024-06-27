using System.IO;
using Kingmaker.Utility.BuildModeUtils;
using UnityEngine;

namespace Kingmaker.Utility.UnityExtensions;

public class ApplicationPaths
{
	public static string dataPath { get; private set; }

	public static string persistentDataPath { get; private set; }

	public static string temporaryCachePath { get; private set; }

	public static string streamingAssetsPath { get; private set; }

	public static string DevelopmentDataPath
	{
		get
		{
			if (Application.isEditor)
			{
				return Path.Combine(Application.dataPath, "..");
			}
			switch (Application.platform)
			{
			case RuntimePlatform.PS4:
				return "/data";
			case RuntimePlatform.PS5:
				return "/devlog/app";
			default:
				if (string.IsNullOrWhiteSpace(Application.consoleLogPath))
				{
					return Application.persistentDataPath;
				}
				return Path.GetDirectoryName(Application.consoleLogPath);
			}
		}
	}

	public static string LogsDir
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(BuildModeUtility.LogsDir))
			{
				return BuildModeUtility.LogsDir;
			}
			if (!Application.isEditor && Application.platform == RuntimePlatform.PS5)
			{
				return persistentDataPath;
			}
			return DevelopmentDataPath;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void Init()
	{
		dataPath = Application.dataPath;
		persistentDataPath = Application.persistentDataPath;
		if (BuildModeUtility.ClientInstanceId != 0)
		{
			persistentDataPath = Path.Combine(persistentDataPath, $"Instance{BuildModeUtility.ClientInstanceId}");
			if (!Directory.Exists(persistentDataPath))
			{
				Directory.CreateDirectory(persistentDataPath);
			}
		}
		temporaryCachePath = Application.temporaryCachePath;
		streamingAssetsPath = Application.streamingAssetsPath;
		PrintPaths();
	}

	public static void PrintPaths()
	{
		Debug.Log("dataPath: " + dataPath);
		Debug.Log("persistentDataPath: " + persistentDataPath);
		Debug.Log("temporaryCachePath: " + temporaryCachePath);
		Debug.Log("streamingAssetsPath: " + streamingAssetsPath);
		Debug.Log("DevelopmentDataPath: " + DevelopmentDataPath);
		Debug.Log("LogsDir: " + LogsDir);
	}
}
