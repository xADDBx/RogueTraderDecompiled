using System.Linq;
using Kingmaker.Logging;
using Kingmaker.Logging.Configuration.Platforms;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker;

public class Switch2LoggingConfiguration : ILoggingConfiguration
{
	private static readonly LogChannelSettings[] Settings = LogChannelSettingsUtils.DefaultSettings.Append(new LogChannelSettings(PFLog.System, LogSeverity.Message, LogSeverity.Exception)).ToArray();

	public void Configure()
	{
		bool isLoggingEnabled = LoggingConfiguration.IsLoggingEnabled;
		Owlcat.Runtime.Core.Logging.Logger.Instance.Enabled = isLoggingEnabled;
		UnityInternalUberLogSink.Enabled = isLoggingEnabled;
		if (!isLoggingEnabled)
		{
			Debug.Log("Storing logs is not enabled");
			return;
		}
		if (!BuildModeUtility.IsDevelopment)
		{
			SuppressLogs();
		}
		Settings.ApplySettings();
		string logsDir = ApplicationPaths.LogsDir;
		Debug.Log("Store logs at: " + logsDir);
		PFLog.Default.Log("Store logs at: " + logsDir);
		Owlcat.Runtime.Core.Logging.Logger.Instance.AddLogger(LogSinkFactory.CreateFull(logsDir, "ConsoleLogFull.txt", backup: true));
		Owlcat.Runtime.Core.Logging.Logger.Instance.AddLogger(LogSinkFactory.CreateHistory());
	}

	private static void SuppressLogs()
	{
		LogChannelDefaults.MinLevel = LogSeverity.Error;
		foreach (string channelName in LogChannelFactory.ChannelNames)
		{
			LogChannelFactory.GetOrCreate(channelName).SetSeverity(LogSeverity.Error);
		}
	}
}
