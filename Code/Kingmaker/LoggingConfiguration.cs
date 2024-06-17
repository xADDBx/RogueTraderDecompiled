using System;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.CommandLineArgs;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker;

public static class LoggingConfiguration
{
	private static ILoggingConfiguration s_Configuration;

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
			CheckCommandLine();
		}
		catch (Exception exception)
		{
			Debug.LogError("Can't initialize logging subsystem");
			Debug.LogException(exception);
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
