using System;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.QA.Arbiter;

public class ArbiterUberLoggerFilter : IDisposableLogSink, ILogSink, IDisposable
{
	private readonly ILogSink m_Logger;

	public ArbiterUberLoggerFilter(ILogSink logger)
	{
		m_Logger = logger;
	}

	public void Log(LogInfo logInfo)
	{
		if (!(logInfo.Channel?.Name != "Arbiter"))
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
