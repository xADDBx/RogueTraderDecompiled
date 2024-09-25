using System;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Utility;

public class ReportingUberLoggerFilter : IDisposableLogSink, ILogSink, IDisposable
{
	private readonly ILogSink m_Logger;

	public ReportingUberLoggerFilter(ILogSink logger)
	{
		m_Logger = logger;
	}

	public void Log(LogInfo logInfo)
	{
		if (!(logInfo.Channel?.Name != "ReportingUtils") || !(logInfo.Channel?.Name != "ReportSender"))
		{
			m_Logger.Log(logInfo);
		}
	}

	public void Destroy()
	{
		(m_Logger as IDisposableLogSink)?.Dispose();
	}

	public void Dispose()
	{
		Destroy();
	}
}
