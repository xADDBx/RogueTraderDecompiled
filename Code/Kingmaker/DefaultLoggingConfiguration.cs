using Kingmaker.Logging;
using Kingmaker.Logging.Configuration.Platforms;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker;

public class DefaultLoggingConfiguration : ILoggingConfiguration
{
	public void Configure()
	{
		Logger.Instance.AddLogger(LogSinkFactory.CreateHistory());
		if (LoggingConfiguration.IsLoggingEnabled)
		{
			LogChannelSettingsUtils.DefaultSettings.ApplySettings();
			string logsDir = ApplicationPaths.LogsDir;
			Logger.Instance.AddLogger(LogSinkFactory.CreateFull(logsDir, "GameLogFull.txt", backup: true));
			Logger.Instance.AddLogger(LogSinkFactory.CreateShort(logsDir, "GameLog.txt", backup: true));
			Logger.Instance.AddLogger(LogSinkFactory.AddSpamDetector());
		}
	}
}
