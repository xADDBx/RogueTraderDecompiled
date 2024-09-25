using System.Text;
using System.Threading;
using UnityEngine;

namespace Owlcat.Runtime.Core.Logging;

public class UnityInternalUberLogSink : ILogSink
{
	private static readonly UnityInternalUberLogSink Instance = new UnityInternalUberLogSink();

	private static readonly ThreadLocal<StringBuilder> StringBuilder = new ThreadLocal<StringBuilder>(() => new StringBuilder());

	public static bool Enabled
	{
		get
		{
			return Logger.Instance.GetLogger<UnityInternalUberLogSink>() != null;
		}
		set
		{
			if (value)
			{
				Logger.Instance.AddLogger(Instance);
			}
			else
			{
				Logger.Instance.RemoveLogger(Instance);
			}
		}
	}

	public void Log(LogInfo logInfo)
	{
		if (UnityUberLogJointSuppressor.IsSuppressed)
		{
			return;
		}
		using (UnityUberLogJointSuppressor.Suppress())
		{
			StringBuilder value = StringBuilder.Value;
			value.AppendFormat("[{0}] {1}", logInfo.Channel.Name, logInfo.Message);
			if (logInfo.Message.Length > 0)
			{
				value.AppendLine();
			}
			if (logInfo.Callstack != null)
			{
				UberLoggerStackTraceUtils2.FormatStack(value, logInfo.Callstack, formatLinks: false);
			}
			UnityInternalForwarding.Log(logInfo.Severity.ToUnity(), LogOption.NoStacktrace, value.ToString(), logInfo.Source as Object);
			value.Clear();
		}
	}

	public void Destroy()
	{
	}
}
