using UnityEngine;

namespace Owlcat.Runtime.Core.Logging;

public static class LogChannelDefaults
{
	public static LogSeverity MinLevel = LogSeverity.Message;

	public static LogSeverity DefaultStackTraceLevel = LogSeverity.Error;

	public static void SetUnityDefaultStackTraceLevel(LogSeverity severity, StackTraceLogType traceLogType = StackTraceLogType.ScriptOnly)
	{
		for (LogSeverity logSeverity = LogSeverity.Message; logSeverity <= LogSeverity.Error; logSeverity++)
		{
			Application.SetStackTraceLogType(logSeverity.ToUnity(), (logSeverity >= severity) ? traceLogType : StackTraceLogType.None);
		}
	}
}
