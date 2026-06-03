using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Logging;

public static class LogChannelSettingsUtils
{
	public static readonly LogChannelSettings[] DefaultSettings = new LogChannelSettings[6]
	{
		new LogChannelSettings(PFLog.Audio, LogSeverity.Message, LogSeverity.Disabled),
		new LogChannelSettings(PFLog.UI, LogSeverity.Message, LogSeverity.Disabled),
		new LogChannelSettings(PFLog.Cutscene, LogSeverity.Message, LogSeverity.Disabled),
		new LogChannelSettings(PFLog.Resources, LogSeverity.Warning, LogSeverity.Disabled),
		new LogChannelSettings(PFLog.Bundles, LogSeverity.Warning, LogSeverity.Disabled),
		new LogChannelSettings(PFLog.TechArt, LogSeverity.Message, LogSeverity.Disabled)
	};

	public static void ApplySettings(this LogChannelSettings[] settings)
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
}
