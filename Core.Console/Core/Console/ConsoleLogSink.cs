using System;
using System.Collections.Generic;
using System.Threading;
using Owlcat.Runtime.Core.Logging;

namespace Core.Console;

public class ConsoleLogSink : ILogSink
{
	private static int s_MaxMessageBuffer = 10000;

	private readonly Queue<LogEntry> m_Entries = new Queue<LogEntry>();

	private static volatile int s_Idx;

	public static ConsoleLogSink Instance { get; set; }

	public static void SetMessageBufferSize(int size)
	{
		s_MaxMessageBuffer = size;
	}

	public void Log(LogInfo entry)
	{
		lock (m_Entries)
		{
			LogEntry item = new LogEntry(Interlocked.Increment(ref s_Idx), entry.TimeStamp, entry.Severity, entry.Channel.Name, entry.Message, (entry.Callstack != null) ? UberLoggerStackTraceUtils.StackToString(entry.Callstack, formatLinks: false) : string.Empty);
			m_Entries.Enqueue(item);
			while (m_Entries.Count > s_MaxMessageBuffer)
			{
				m_Entries.Dequeue();
			}
		}
	}

	public static LogEntry[] Poll(Guid consoleGuid)
	{
		return Instance?.PollImpl() ?? Array.Empty<LogEntry>();
	}

	public LogEntry[] PollImpl()
	{
		lock (m_Entries)
		{
			LogEntry[] result = m_Entries.ToArray();
			m_Entries.Clear();
			return result;
		}
	}

	public void Destroy()
	{
	}
}
