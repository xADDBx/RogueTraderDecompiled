using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Owlcat.Runtime.Core.Logging;

public class LogChannelFactory
{
	private static readonly Dictionary<string, LogChannel> Channels = new Dictionary<string, LogChannel>();

	public static List<string> ChannelNames => Channels.Keys.ToList();

	public static LogChannel GetOrCreate([CanBeNull] string name, int sinkBitmap = 0, LogSeverity minLevel = LogSeverity.Message, LogSeverity minStackTraceLevel = LogSeverity.Warning)
	{
		lock (Channels)
		{
			if (name == null)
			{
				return LogChannel.Default;
			}
			if (!Channels.TryGetValue(name, out var value))
			{
				value = Create(name, sinkBitmap, minLevel, minStackTraceLevel);
			}
			return value;
		}
	}

	protected internal static LogChannel Create([NotNull] string name, int sinkBitmap = 0, LogSeverity minLevel = LogSeverity.Message, LogSeverity minStackTraceLevel = LogSeverity.Warning)
	{
		lock (Channels)
		{
			if (Channels.ContainsKey(name))
			{
				throw new ArgumentException("LogChannel with name " + name + " already exists");
			}
			LogChannel logChannel = new LogChannel(name, sinkBitmap, minLevel, minStackTraceLevel);
			Channels[name] = logChannel;
			return logChannel;
		}
	}
}
