using System;
using Owlcat.Runtime.Core.Logging;

namespace Core.Console;

public readonly struct LogEntry
{
	public readonly int Index;

	public readonly DateTime Timestamp;

	public readonly LogSeverity Level;

	public readonly string Channel;

	public readonly string Message;

	public readonly string StackTrace;

	public LogEntry(int index, DateTime timestamp, LogSeverity level, string channel, string message, string stackTrace)
	{
		Index = index;
		Timestamp = timestamp;
		Level = level;
		Channel = channel;
		Message = message;
		StackTrace = stackTrace;
	}
}
