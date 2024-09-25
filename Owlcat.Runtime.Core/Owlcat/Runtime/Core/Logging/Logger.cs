using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace Owlcat.Runtime.Core.Logging;

public class Logger
{
	private struct IgnoredUnityMethod
	{
		public enum Mode
		{
			Show,
			ShowIfFirstIgnoredMethod,
			Hide
		}

		public string DeclaringTypeName;

		public string MethodName;

		public Mode ShowHideMode;
	}

	public bool Enabled = true;

	public static readonly int MaxMessagesToKeep = 100;

	public static readonly string UnityInternalNewLine = "\n";

	private readonly List<ILogSink> m_Loggers = new List<ILogSink>();

	private readonly List<IDisposableLogSink> m_DisposableLogSinks = new List<IDisposableLogSink>();

	private bool m_AlreadyLogging;

	private static readonly IgnoredUnityMethod[] IgnoredUnityMethods = new IgnoredUnityMethod[6]
	{
		new IgnoredUnityMethod
		{
			DeclaringTypeName = "Application",
			MethodName = "CallLogCallback",
			ShowHideMode = IgnoredUnityMethod.Mode.Hide
		},
		new IgnoredUnityMethod
		{
			DeclaringTypeName = "DebugLogHandler",
			MethodName = null,
			ShowHideMode = IgnoredUnityMethod.Mode.Hide
		},
		new IgnoredUnityMethod
		{
			DeclaringTypeName = "Logger",
			MethodName = null,
			ShowHideMode = IgnoredUnityMethod.Mode.ShowIfFirstIgnoredMethod
		},
		new IgnoredUnityMethod
		{
			DeclaringTypeName = "LogChannel",
			MethodName = null,
			ShowHideMode = IgnoredUnityMethod.Mode.Hide
		},
		new IgnoredUnityMethod
		{
			DeclaringTypeName = "Debug",
			MethodName = null,
			ShowHideMode = IgnoredUnityMethod.Mode.ShowIfFirstIgnoredMethod
		},
		new IgnoredUnityMethod
		{
			DeclaringTypeName = "Assert",
			MethodName = null,
			ShowHideMode = IgnoredUnityMethod.Mode.ShowIfFirstIgnoredMethod
		}
	};

	private static Logger s_Instance;

	[NotNull]
	public static Logger Instance
	{
		get
		{
			if (s_Instance == null)
			{
				InitializeLogger();
			}
			return s_Instance;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void InitializeLogger()
	{
		if (s_Instance == null)
		{
			s_Instance = new Logger();
		}
	}

	public static void TestReinitialize()
	{
		if (s_Instance != null)
		{
			s_Instance = null;
		}
		InitializeLogger();
	}

	public static DateTime GetTime()
	{
		return DateTime.Now;
	}

	public void AddLogger(IDisposableLogSink logSink, bool populateWithExistingMessages = true)
	{
		lock (m_Loggers)
		{
			AddLogger((ILogSink)logSink, populateWithExistingMessages);
			m_DisposableLogSinks.Add(logSink);
		}
	}

	public void AddLogger(ILogSink logSink, bool populateWithExistingMessages = true)
	{
		lock (m_Loggers)
		{
			if (!m_Loggers.Contains(logSink))
			{
				m_Loggers.Add(logSink);
			}
		}
	}

	public void RemoveLogger(ILogSink logger)
	{
		lock (m_Loggers)
		{
			if (m_Loggers.Contains(logger))
			{
				m_Loggers.Remove(logger);
				logger.Destroy();
			}
		}
	}

	public void RemoveAllLogger<T>()
	{
		lock (m_Loggers)
		{
			for (int i = 0; i < m_Loggers.Count; i++)
			{
				if (m_Loggers[i] is T)
				{
					m_Loggers[i].Destroy();
					m_Loggers[i] = null;
				}
			}
			m_Loggers.RemoveAll((ILogSink l) => l == null);
		}
	}

	public void DisposeLogSinks()
	{
		lock (m_Loggers)
		{
			List<IDisposableLogSink> disposedSinks = m_DisposableLogSinks.Where(TryDispose).ToList();
			m_Loggers.RemoveAll((ILogSink sink) => disposedSinks.Contains(sink));
			m_DisposableLogSinks.RemoveAll((IDisposableLogSink sink) => disposedSinks.Contains(sink));
		}
	}

	private static bool TryDispose(IDisposableLogSink sink)
	{
		try
		{
			sink.Dispose();
			return true;
		}
		catch (Exception ex)
		{
			LogChannel.Default.Exception(ex, $"Failed to dispose {sink}");
			return false;
		}
	}

	public static bool ExtractInfoFromUnityStackInfo(string log, out string declaringType, out string methodName, out string filename, out int lineNumber)
	{
		declaringType = null;
		methodName = null;
		filename = null;
		lineNumber = 0;
		ReadOnlyMemory<char> readOnlyMemory = log.AsMemory();
		int num = readOnlyMemory.Span.IndexOf(":", StringComparison.CurrentCulture);
		if (num < 0)
		{
			return false;
		}
		ReadOnlyMemory<char> readOnlyMemory2 = readOnlyMemory;
		ReadOnlyMemory<char> readOnlyMemory3 = readOnlyMemory2.Slice(0, num);
		int num2 = num + 1;
		readOnlyMemory2 = readOnlyMemory;
		int num3 = num2;
		readOnlyMemory = readOnlyMemory2.Slice(num3, readOnlyMemory2.Length - num3);
		int num4 = readOnlyMemory.Span.IndexOf(" (");
		if (num4 < 0)
		{
			return false;
		}
		readOnlyMemory2 = readOnlyMemory;
		ReadOnlyMemory<char> readOnlyMemory4 = readOnlyMemory2.Slice(0, num4);
		readOnlyMemory2 = readOnlyMemory;
		num3 = num4 + 2;
		readOnlyMemory = readOnlyMemory2.Slice(num3, readOnlyMemory2.Length - num3);
		int num5 = readOnlyMemory.Span.LastIndexOf(")");
		if (num5 < 0)
		{
			return false;
		}
		readOnlyMemory2 = readOnlyMemory;
		readOnlyMemory = readOnlyMemory2.Slice(0, num5);
		int num6 = readOnlyMemory.Span.LastIndexOf(":");
		if (num6 < 0)
		{
			return false;
		}
		int num7 = num6 + 1;
		readOnlyMemory2 = readOnlyMemory;
		num3 = num7;
		ReadOnlyMemory<char> readOnlyMemory5 = readOnlyMemory2.Slice(num3, readOnlyMemory2.Length - num3);
		readOnlyMemory2 = readOnlyMemory;
		readOnlyMemory = readOnlyMemory2.Slice(0, num6);
		int num8 = readOnlyMemory.Span.LastIndexOf("(at ");
		if (num8 < 0)
		{
			return false;
		}
		readOnlyMemory2 = readOnlyMemory;
		num3 = num8 + 4;
		ReadOnlyMemory<char> readOnlyMemory6 = readOnlyMemory2.Slice(num3, readOnlyMemory2.Length - num3);
		declaringType = readOnlyMemory3.ToString();
		methodName = readOnlyMemory4.ToString();
		filename = readOnlyMemory6.ToString();
		return int.TryParse(readOnlyMemory5.Span, out lineNumber);
	}

	private static IgnoredUnityMethod.Mode ShowOrHideMethod(MethodBase method)
	{
		IgnoredUnityMethod[] ignoredUnityMethods = IgnoredUnityMethods;
		for (int i = 0; i < ignoredUnityMethods.Length; i++)
		{
			IgnoredUnityMethod ignoredUnityMethod = ignoredUnityMethods[i];
			if (method.DeclaringType.Name == ignoredUnityMethod.DeclaringTypeName && (ignoredUnityMethod.MethodName == null || method.Name == ignoredUnityMethod.MethodName))
			{
				return ignoredUnityMethod.ShowHideMode;
			}
		}
		return IgnoredUnityMethod.Mode.Show;
	}

	[StackTraceIgnore]
	public void Log([NotNull] LogChannel channel, object source, LogSeverity severity, Exception ex, [CanBeNull] string message, object[] par)
	{
		if (Enabled && severity >= channel.MinLevel)
		{
			List<LogStackFrame> list = new List<LogStackFrame>();
			if (ex != null)
			{
				FormatException(ex, list);
			}
			if (severity >= channel.MinStackTraceLevel)
			{
				UberLoggerStackTraceUtils2.GetCallstack(list, new StackTrace(fNeedFileInfo: true));
			}
			string message2 = ((message == null) ? "" : ((par != null && par.Length > 0) ? string.Format(message, par) : message));
			LogInfo logInfo = new LogInfo(source, channel, GetTime(), severity, list, message2);
			Log(logInfo);
		}
	}

	[StackTraceIgnore]
	public void Log(LogInfo logInfo)
	{
		if (!Enabled || logInfo.Severity < logInfo.Channel.MinLevel)
		{
			return;
		}
		lock (m_Loggers)
		{
			if (m_AlreadyLogging)
			{
				return;
			}
			m_AlreadyLogging = true;
			try
			{
				m_Loggers.RemoveAll((ILogSink l) => l == null);
				foreach (ILogSink logger in m_Loggers)
				{
					logger.Log(logInfo);
				}
			}
			finally
			{
				m_AlreadyLogging = false;
			}
		}
	}

	private static void FormatException(Exception e, List<LogStackFrame> frames)
	{
		VisitInner(e, frames);
		frames.Add(new LogStackFrame(" --- End of Exception Stack Trace ---", "", 0));
	}

	private static void VisitInner(Exception e, List<LogStackFrame> frames)
	{
		Exception innerException = e.InnerException;
		if (innerException != null)
		{
			VisitInner(innerException, frames);
			frames.Add(new LogStackFrame(" --- Rethrow ---", "", 0));
		}
		string message = (string.IsNullOrWhiteSpace(e.Message) ? e.GetType().ToString() : $"{e.GetType()}: {e.Message}");
		frames.Add(new LogStackFrame(message, "", 0));
		StackTrace stackTrace = new StackTrace(e, fNeedFileInfo: true);
		UberLoggerStackTraceUtils2.GetCallstack(frames, stackTrace);
	}

	public T GetLogger<T>() where T : class
	{
		foreach (ILogSink logger in m_Loggers)
		{
			if (logger is T)
			{
				return logger as T;
			}
		}
		return null;
	}
}
