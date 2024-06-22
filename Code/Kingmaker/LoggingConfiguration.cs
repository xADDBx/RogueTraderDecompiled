using System;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.CommandLineArgs;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker;

public static class LoggingConfiguration
{
	private class LogChannelSettings
	{
		public readonly LogChannel Channel;

		public readonly LogSeverity MinLevel;

		public readonly LogSeverity MinStackTraceLevel;

		public LogChannelSettings(LogChannel channel, LogSeverity minLevel, LogSeverity minStackTraceLevel)
		{
			Channel = channel;
			MinLevel = minLevel;
			MinStackTraceLevel = minStackTraceLevel;
		}
	}

	private static ILoggingConfiguration s_Configuration;

	private static readonly LogChannelSettings[] LogChannelsSettings = new LogChannelSettings[6]
	{
		new LogChannelSettings(PFLog.Audio, LogSeverity.Message, LogSeverity.Disabled),
		new LogChannelSettings(PFLog.UI, LogSeverity.Message, LogSeverity.Disabled),
		new LogChannelSettings(PFLog.Cutscene, LogSeverity.Message, LogSeverity.Disabled),
		new LogChannelSettings(PFLog.Resources, LogSeverity.Warning, LogSeverity.Disabled),
		new LogChannelSettings(PFLog.Bundles, LogSeverity.Warning, LogSeverity.Disabled),
		new LogChannelSettings(PFLog.TechArt, LogSeverity.Message, LogSeverity.Disabled)
	};

	public static bool IsLoggingEnabled
	{
		get
		{
			if (!CommandLineArguments.Parse().Contains("logging") && BuildModeUtility.IsRelease)
			{
				return BuildModeUtility.Data.ForceLogging;
			}
			return true;
		}
	}

	public static void Configure()
	{
		if (s_Configuration != null)
		{
			return;
		}
		s_Configuration = LoggingConfigurationProvider.PickConfiguration();
		try
		{
			s_Configuration.Configure();
			ApplyLogChannelsSettings(LogChannelsSettings);
			CheckCommandLine();
		}
		catch (Exception exception)
		{
			Debug.LogError("Can't initialize logging subsystem");
			Debug.LogException(exception);
		}
	}

	private static void ApplyLogChannelsSettings(LogChannelSettings[] settings)
	{
		if (!Application.isEditor)
		{
			foreach (LogChannelSettings logChannelSettings in settings)
			{
				logChannelSettings.Channel.SetSeverity(logChannelSettings.MinLevel);
				logChannelSettings.Channel.SetMinStackTraceLevel(logChannelSettings.MinStackTraceLevel);
			}
		}
	}

	private static void CheckCommandLine()
	{
		string text = CommandLineArguments.Parse().Get("disableLogChannel");
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		string[] array = text.Split(',');
		foreach (string text2 in array)
		{
			if (LogChannelFactory.ChannelNames.Contains(text2))
			{
				LogChannelFactory.GetOrCreate(text2).SetSeverity(LogSeverity.Disabled);
			}
		}
	}
}
