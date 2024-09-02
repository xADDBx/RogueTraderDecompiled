using Kingmaker.Logging.Configuration.Platforms;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker;

public class DefaultLoggingConfiguration : ILoggingConfiguration
{
	public void Configure()
	{
		Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
		Owlcat.Runtime.Core.Logging.Logger.Instance.AddLogger(LogSinkFactory.CreateHistory());
		if (LoggingConfiguration.IsLoggingEnabled)
		{
			string logsDir = ApplicationPaths.LogsDir;
			Owlcat.Runtime.Core.Logging.Logger.Instance.AddLogger(LogSinkFactory.CreateFull(logsDir, "GameLogFull.txt", backup: true));
			Owlcat.Runtime.Core.Logging.Logger.Instance.AddLogger(LogSinkFactory.CreateShort(logsDir, "GameLog.txt", backup: true));
			Owlcat.Runtime.Core.Logging.Logger.Instance.AddLogger(LogSinkFactory.AddSpamDetector());
		}
	}
}
