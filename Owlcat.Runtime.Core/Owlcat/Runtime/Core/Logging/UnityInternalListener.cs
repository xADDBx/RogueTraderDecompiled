using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Owlcat.Runtime.Core.Logging;

public static class UnityInternalListener
{
	private static readonly Regex UnityMessageRegex = new Regex("(.*)\\((\\d+).*\\)", RegexOptions.Compiled);

	public const string UnityInternalNewLine = "\n";

	private static bool s_Initialized;

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		if (!s_Initialized)
		{
			Application.logMessageReceivedThreaded += UnityLogInternal;
			s_Initialized = true;
		}
	}

	[StackTraceIgnore]
	private static void UnityLogInternal(string unityMessage, string unityCallStack, LogType logType)
	{
		if (UnityUberLogJointSuppressor.IsSuppressed)
		{
			return;
		}
		using (UnityUberLogJointSuppressor.Suppress())
		{
			List<LogStackFrame> list;
			if (string.IsNullOrWhiteSpace(unityCallStack))
			{
				list = new List<LogStackFrame>();
				UberLoggerStackTraceUtils2.GetCallstack(list, new StackTrace(fNeedFileInfo: true));
			}
			else
			{
				list = GetCallstackFromUnityLog(unityCallStack);
			}
			LogSeverity severity = logType.FromUnity();
			string filename = "";
			int lineNumber = 0;
			if (ExtractInfoFromUnityMessage(unityMessage, ref filename, ref lineNumber))
			{
				list.Insert(0, new LogStackFrame(unityMessage, filename, lineNumber));
			}
			LogInfo logInfo = new LogInfo(null, LogChannel.Unity, Logger.GetTime(), severity, list, unityMessage);
			Logger.Instance.Log(logInfo);
		}
	}

	private static bool ExtractInfoFromUnityMessage(string log, ref string filename, ref int lineNumber)
	{
		Match match = UnityMessageRegex.Match(log);
		if (match.Success)
		{
			filename = match.Groups[1].Value;
			int.TryParse(match.Groups[2].Value, out lineNumber);
			return true;
		}
		return false;
	}

	private static List<LogStackFrame> GetCallstackFromUnityLog(string unityCallstack)
	{
		return (from line in unityCallstack.Split(new string[1] { "\n" }, StringSplitOptions.None)
			select new LogStackFrame(line) into frame
			where !string.IsNullOrEmpty(frame.GetFormattedMethodName())
			select frame).ToList();
	}
}
