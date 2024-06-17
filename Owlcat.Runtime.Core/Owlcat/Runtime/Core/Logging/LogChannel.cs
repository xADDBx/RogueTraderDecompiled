using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Kingmaker.Utility;
using UnityEngine;

namespace Owlcat.Runtime.Core.Logging;

[Serializable]
public class LogChannel
{
	public static readonly LogChannel Audio = LogChannelFactory.Create("Audio");

	public static readonly LogChannel Unity = LogChannelFactory.Create("Unity");

	public static readonly LogChannel System = LogChannelFactory.Create("System");

	public static readonly LogChannel TechArt = LogChannelFactory.Create("TechArt");

	public static readonly LogChannel Default = LogChannelFactory.Create("Default");

	public static readonly LogChannel Resources = LogChannelFactory.Create("Resources");

	public static readonly LogChannel Craft = LogChannelFactory.Create("Craft");

	public static readonly LogChannel Mods = LogChannelFactory.Create("Mods");

	[SerializeField]
	private string m_Name;

	[SerializeField]
	private int m_SinkBitmap;

	[SerializeField]
	private LogSeverity m_MinLevel;

	[SerializeField]
	private LogSeverity m_MinStackTraceLevel;

	public string Name => m_Name;

	public int SinkBitmap => m_SinkBitmap;

	public LogSeverity MinLevel => m_MinLevel;

	public LogSeverity MinStackTraceLevel => m_MinStackTraceLevel;

	public void SetSeverity(LogSeverity severity)
	{
		m_MinLevel = severity;
	}

	internal LogChannel(string name, int sinkBitmap, LogSeverity minLevel, LogSeverity minStackTraceLevel)
	{
		m_Name = name;
		m_MinLevel = minLevel;
		m_SinkBitmap = sinkBitmap;
		m_MinStackTraceLevel = minStackTraceLevel;
	}

	[StackTraceIgnore]
	public void Log(string messageFormat)
	{
		Logger.Instance.Log(this, null, LogSeverity.Message, null, messageFormat, Array.Empty<object>());
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	public void Log(string messageFormat, params object[] @params)
	{
		Logger.Instance.Log(this, null, LogSeverity.Message, null, messageFormat, @params);
	}

	[StackTraceIgnore]
	public void Log(UnityEngine.Object ctx, string messageFormat = null)
	{
		Logger.Instance.Log(this, ctx, LogSeverity.Message, null, messageFormat, Array.Empty<object>());
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	public void Log(UnityEngine.Object ctx, string messageFormat, params object[] @params)
	{
		Logger.Instance.Log(this, ctx, LogSeverity.Message, null, messageFormat, @params);
	}

	[StackTraceIgnore]
	public void Log(ICanBeLogContext ctx, string messageFormat = null)
	{
		Logger.Instance.Log(this, ctx, LogSeverity.Message, null, messageFormat, Array.Empty<object>());
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	public void Log(ICanBeLogContext ctx, string messageFormat, params object[] @params)
	{
		Logger.Instance.Log(this, ctx, LogSeverity.Message, null, messageFormat, @params);
	}

	[StackTraceIgnore]
	public void Log(Exception ex, string messageFormat = null)
	{
		if (ex is OwlcatException { IsReportingNow: false } ex2)
		{
			ex2.Log(this);
		}
		else
		{
			Logger.Instance.Log(this, null, LogSeverity.Message, ex, messageFormat, Array.Empty<object>());
		}
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	public void Log(Exception ex, string messageFormat, params object[] @params)
	{
		if (ex is OwlcatException { IsReportingNow: false } ex2)
		{
			ex2.Log(this);
		}
		else
		{
			Logger.Instance.Log(this, null, LogSeverity.Message, ex, messageFormat, @params);
		}
	}

	[StackTraceIgnore]
	[Conditional("VERBOSE_LOGS")]
	public void Verbose(string messageFormat)
	{
		Logger.Instance.Log(this, null, LogSeverity.Message, null, messageFormat, Array.Empty<object>());
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	[Conditional("VERBOSE_LOGS")]
	public void Verbose(string messageFormat, params object[] @params)
	{
		Logger.Instance.Log(this, null, LogSeverity.Message, null, messageFormat, @params);
	}

	[StackTraceIgnore]
	[Conditional("VERBOSE_LOGS")]
	public void Verbose(UnityEngine.Object ctx, string messageFormat = null)
	{
		Logger.Instance.Log(this, ctx, LogSeverity.Message, null, messageFormat, Array.Empty<object>());
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	[Conditional("VERBOSE_LOGS")]
	public void Verbose(UnityEngine.Object ctx, string messageFormat, params object[] @params)
	{
		Logger.Instance.Log(this, ctx, LogSeverity.Message, null, messageFormat, @params);
	}

	[StackTraceIgnore]
	public void Verbose(Exception ex, string messageFormat = null)
	{
		if (ex is OwlcatException { IsReportingNow: false } ex2)
		{
			ex2.Log(this);
		}
		else
		{
			Logger.Instance.Log(this, null, LogSeverity.Message, ex, messageFormat, Array.Empty<object>());
		}
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	public void Verbose(Exception ex, string messageFormat, params object[] @params)
	{
		if (ex is OwlcatException { IsReportingNow: false } ex2)
		{
			ex2.Log(this);
		}
		else
		{
			Logger.Instance.Log(this, null, LogSeverity.Message, ex, messageFormat, @params);
		}
	}

	[StackTraceIgnore]
	public void Warning(string messageFormat)
	{
		Logger.Instance.Log(this, null, LogSeverity.Warning, null, messageFormat, Array.Empty<object>());
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	public void Warning(string messageFormat, params object[] @params)
	{
		Logger.Instance.Log(this, null, LogSeverity.Warning, null, messageFormat, @params);
	}

	[StackTraceIgnore]
	public void Warning(ICanBeLogContext ctx, string messageFormat = null)
	{
		Logger.Instance.Log(this, ctx, LogSeverity.Warning, null, messageFormat, Array.Empty<object>());
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	public void Warning(ICanBeLogContext ctx, string messageFormat, params object[] @params)
	{
		Logger.Instance.Log(this, ctx, LogSeverity.Warning, null, messageFormat, @params);
	}

	[StackTraceIgnore]
	public void Warning(UnityEngine.Object ctx, string messageFormat = null)
	{
		Logger.Instance.Log(this, ctx, LogSeverity.Warning, null, messageFormat, Array.Empty<object>());
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	public void Warning(UnityEngine.Object ctx, string messageFormat, params object[] @params)
	{
		Logger.Instance.Log(this, ctx, LogSeverity.Warning, null, messageFormat, @params);
	}

	[StackTraceIgnore]
	public void Warning(Exception ex, string messageFormat = null)
	{
		if (ex is OwlcatException { IsReportingNow: false } ex2)
		{
			ex2.Log(this);
		}
		else
		{
			Logger.Instance.Log(this, null, LogSeverity.Warning, ex, messageFormat, Array.Empty<object>());
		}
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	public void Warning(Exception ex, string messageFormat, params object[] @params)
	{
		if (ex is OwlcatException { IsReportingNow: false } ex2)
		{
			ex2.Log(this);
		}
		else
		{
			Logger.Instance.Log(this, null, LogSeverity.Warning, ex, messageFormat, @params);
		}
	}

	[StackTraceIgnore]
	public void Error(string messageFormat)
	{
		Logger.Instance.Log(this, null, LogSeverity.Error, null, messageFormat, Array.Empty<object>());
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	public void Error(string messageFormat, params object[] @params)
	{
		Logger.Instance.Log(this, null, LogSeverity.Error, null, messageFormat, @params);
	}

	[StackTraceIgnore]
	public void Error(UnityEngine.Object ctx, string messageFormat = null)
	{
		Logger.Instance.Log(this, ctx, LogSeverity.Error, null, messageFormat, Array.Empty<object>());
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	public void Error(UnityEngine.Object ctx, string messageFormat, params object[] @params)
	{
		Logger.Instance.Log(this, ctx, LogSeverity.Error, null, messageFormat, @params);
	}

	[StackTraceIgnore]
	public void Error(ICanBeLogContext ctx, string messageFormat = null)
	{
		Logger.Instance.Log(this, ctx, LogSeverity.Error, null, messageFormat, Array.Empty<object>());
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	public void Error(ICanBeLogContext ctx, string messageFormat, params object[] @params)
	{
		Logger.Instance.Log(this, ctx, LogSeverity.Error, null, messageFormat, @params);
	}

	[StackTraceIgnore]
	public void Error(Exception ex, string messageFormat = null)
	{
		if (ex is OwlcatException { IsReportingNow: false } ex2)
		{
			ex2.Log(this);
		}
		else
		{
			Logger.Instance.Log(this, null, LogSeverity.Error, ex, messageFormat, Array.Empty<object>());
		}
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	public void Error(Exception ex, string messageFormat, params object[] @params)
	{
		if (ex is OwlcatException { IsReportingNow: false } ex2)
		{
			ex2.Log(this);
		}
		else
		{
			Logger.Instance.Log(this, null, LogSeverity.Error, ex, messageFormat, @params);
		}
	}

	[StackTraceIgnore]
	public void Exception(Exception ex, string messageFormat = null)
	{
		if (ex is OwlcatException { IsReportingNow: false } ex2)
		{
			ex2.Log(this);
		}
		else
		{
			Logger.Instance.Log(this, null, LogSeverity.Error, ex, messageFormat, Array.Empty<object>());
		}
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	public void Exception(Exception ex, string messageFormat, params object[] @params)
	{
		if (ex is OwlcatException { IsReportingNow: false } ex2)
		{
			ex2.Log(this);
		}
		else
		{
			Logger.Instance.Log(this, null, LogSeverity.Error, ex, messageFormat, @params);
		}
	}

	[StackTraceIgnore]
	public void Exception(UnityEngine.Object ctx, Exception ex, string messageFormat = null)
	{
		if (ex is OwlcatException { IsReportingNow: false } ex2)
		{
			ex2.Log(this, ctx);
		}
		else
		{
			Logger.Instance.Log(this, ctx, LogSeverity.Error, ex, messageFormat, Array.Empty<object>());
		}
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	public void Exception(UnityEngine.Object ctx, Exception ex, string messageFormat, params object[] @params)
	{
		if (ex is OwlcatException { IsReportingNow: false } ex2)
		{
			ex2.Log(this, ctx);
		}
		else
		{
			Logger.Instance.Log(this, ctx, LogSeverity.Error, ex, messageFormat, @params);
		}
	}

	[StackTraceIgnore]
	public void Exception(ICanBeLogContext ctx, Exception ex, string messageFormat = null)
	{
		if (ex is OwlcatException { IsReportingNow: false } ex2)
		{
			ex2.Log(this);
		}
		else
		{
			Logger.Instance.Log(this, ctx, LogSeverity.Error, ex, messageFormat, Array.Empty<object>());
		}
	}

	[StackTraceIgnore]
	[StringFormatMethod("messageFormat")]
	public void Exception(ICanBeLogContext ctx, Exception ex, string messageFormat, params object[] @params)
	{
		if (ex is OwlcatException { IsReportingNow: false } ex2)
		{
			ex2.Log(this);
		}
		else
		{
			Logger.Instance.Log(this, ctx, LogSeverity.Error, ex, messageFormat, @params);
		}
	}
}
