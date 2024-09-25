using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.AI.DebugUtilities;

public class AILogger
{
	private readonly struct LogInfo
	{
		public readonly DateTime time;

		public readonly AILogObject log;

		public LogInfo(DateTime time, AILogObject log)
		{
			this.time = time;
			this.log = log;
		}
	}

	private static AILogger s_Instance;

	private readonly List<LogInfo> m_Logs = new List<LogInfo>();

	public static AILogger Instance => s_Instance;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void InitializeLogger()
	{
		if (s_Instance == null)
		{
			s_Instance = new AILogger();
		}
	}

	public void Log(AILogObject logObject)
	{
		PFLog.AI.Log(logObject.GetLogString());
	}

	public void Error(AILogObject logObject)
	{
		PFLog.AI.Error(logObject.GetLogString());
	}

	public void Exception(Exception ex, string message)
	{
		PFLog.AI.Exception(ex, message);
	}

	public void ClearAll()
	{
	}

	private static string GetLogString(LogInfo logInfo)
	{
		return $"[{logInfo.time:dd.MM.yyyy HH:mm:ss:fff}]: {logInfo.log.GetLogString()}";
	}
}
