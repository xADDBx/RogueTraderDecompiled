using Kingmaker.Logging.Configuration.Platforms;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker;

public class ConsoleLoggingConfiguration : ILoggingConfiguration
{
	private readonly bool m_enableLogsForwardingToUnity;

	public ConsoleLoggingConfiguration(bool enableLogsForwardingToUnity)
	{
		m_enableLogsForwardingToUnity = enableLogsForwardingToUnity;
	}

	public void Configure()
	{
		Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
		Owlcat.Runtime.Core.Logging.Logger.Instance.Enabled = LoggingConfiguration.IsLoggingEnabled;
		UnityInternalUberLogSink.Enabled = m_enableLogsForwardingToUnity;
		if (!Owlcat.Runtime.Core.Logging.Logger.Instance.Enabled)
		{
			Debug.Log("Storing logs is not enabled");
			return;
		}
		string logsDir = ApplicationPaths.LogsDir;
		Debug.Log("Store logs at: " + logsDir);
		PFLog.Default.Log("Store logs at: " + logsDir);
		Owlcat.Runtime.Core.Logging.Logger.Instance.AddLogger(LogSinkFactory.CreateFull(logsDir, "ConsoleLogFull.txt", backup: true));
		Owlcat.Runtime.Core.Logging.Logger.Instance.AddLogger(LogSinkFactory.CreateHistory());
	}
}
