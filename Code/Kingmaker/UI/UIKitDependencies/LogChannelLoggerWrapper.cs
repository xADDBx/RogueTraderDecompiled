using System;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.UI.UIKitDependencies;

public class LogChannelLoggerWrapper : ILogger, ILogHandler
{
	private readonly string m_Tag;

	private LogChannel m_LogChannel;

	public ILogHandler logHandler
	{
		get
		{
			return this;
		}
		set
		{
		}
	}

	public bool logEnabled
	{
		get
		{
			return m_LogChannel.MinLevel != LogSeverity.Disabled;
		}
		set
		{
			m_LogChannel.SetSeverity(GetLogSeverity(filterLogType, value));
		}
	}

	public LogType filterLogType
	{
		get
		{
			return GetLogType(m_LogChannel.MinLevel);
		}
		set
		{
			m_LogChannel.SetSeverity(GetLogSeverity(value, logEnabled));
		}
	}

	public LogChannelLoggerWrapper(LogChannel logChannel, string tag)
	{
		m_LogChannel = logChannel;
		m_Tag = tag;
	}

	[StackTraceIgnore]
	public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
	{
		Log(logType, (object)string.Format(format, args), context);
	}

	[StackTraceIgnore]
	public void LogFormat(LogType logType, string format, params object[] args)
	{
		Log(logType, string.Format(format, args));
	}

	[StackTraceIgnore]
	public void LogException(Exception exception, UnityEngine.Object context)
	{
		m_LogChannel.Exception(context, exception);
	}

	public bool IsLogTypeAllowed(LogType logType)
	{
		return m_LogChannel.MinLevel >= GetLogSeverity(logType);
	}

	[StackTraceIgnore]
	public void Log(LogType logType, object message)
	{
		switch (logType)
		{
		case LogType.Error:
		case LogType.Exception:
			LogError(m_Tag, message);
			break;
		case LogType.Assert:
			LogError(m_Tag, message);
			break;
		case LogType.Warning:
			LogWarning(m_Tag, message);
			break;
		case LogType.Log:
			Log(message.ToString());
			break;
		}
	}

	[StackTraceIgnore]
	public void Log(LogType logType, object message, UnityEngine.Object context)
	{
		switch (logType)
		{
		case LogType.Error:
		case LogType.Exception:
			LogError(m_Tag, message, context);
			break;
		case LogType.Assert:
			LogError(m_Tag, message, context);
			break;
		case LogType.Warning:
			LogWarning(m_Tag, message, context);
			break;
		case LogType.Log:
			Log(m_Tag, message, context);
			break;
		}
	}

	[StackTraceIgnore]
	public void Log(LogType logType, string tag, object message)
	{
		switch (logType)
		{
		case LogType.Error:
		case LogType.Exception:
			LogError(tag, message);
			break;
		case LogType.Assert:
			LogError(tag, message);
			break;
		case LogType.Warning:
			LogWarning(tag, message);
			break;
		case LogType.Log:
			Log(tag, message);
			break;
		}
	}

	[StackTraceIgnore]
	public void Log(LogType logType, string tag, object message, UnityEngine.Object context)
	{
		switch (logType)
		{
		case LogType.Error:
		case LogType.Exception:
			LogError(tag, message, context);
			break;
		case LogType.Assert:
			LogError(tag, message, context);
			break;
		case LogType.Warning:
			LogWarning(tag, message, context);
			break;
		case LogType.Log:
			Log(tag, message, context);
			break;
		}
	}

	[StackTraceIgnore]
	public void Log(object message)
	{
		m_LogChannel.Log(message.ToString());
	}

	[StackTraceIgnore]
	public void Log(string tag, object message)
	{
		m_LogChannel.Log(message.ToString());
	}

	[StackTraceIgnore]
	public void Log(string tag, object message, UnityEngine.Object context)
	{
		m_LogChannel.Log(context, message.ToString());
	}

	[StackTraceIgnore]
	public void LogWarning(string tag, object message)
	{
		m_LogChannel.Warning(message.ToString());
	}

	[StackTraceIgnore]
	public void LogWarning(string tag, object message, UnityEngine.Object context)
	{
		m_LogChannel.Warning(context, message.ToString());
	}

	[StackTraceIgnore]
	public void LogError(string tag, object message)
	{
		m_LogChannel.Error(message.ToString());
	}

	[StackTraceIgnore]
	public void LogError(string tag, object message, UnityEngine.Object context)
	{
		m_LogChannel.Error(context, message.ToString());
	}

	[StackTraceIgnore]
	public void LogException(Exception exception)
	{
		m_LogChannel.Exception(exception);
	}

	private LogSeverity GetLogSeverity(LogType logType, bool enabled = true)
	{
		if (!enabled)
		{
			return LogSeverity.Disabled;
		}
		return logType switch
		{
			LogType.Error => LogSeverity.Error, 
			LogType.Assert => LogSeverity.Error, 
			LogType.Warning => LogSeverity.Warning, 
			LogType.Log => LogSeverity.Message, 
			LogType.Exception => LogSeverity.Error, 
			_ => LogSeverity.Disabled, 
		};
	}

	private LogType GetLogType(LogSeverity logSeverity)
	{
		return logSeverity switch
		{
			LogSeverity.Message => LogType.Log, 
			LogSeverity.Warning => LogType.Warning, 
			LogSeverity.Error => LogType.Error, 
			LogSeverity.Disabled => LogType.Exception, 
			_ => LogType.Exception, 
		};
	}
}
