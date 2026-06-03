using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Logging;

public class LogChannelSettings
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
