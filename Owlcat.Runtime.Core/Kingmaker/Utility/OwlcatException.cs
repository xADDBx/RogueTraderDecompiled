using System;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Utility;

public abstract class OwlcatException : Exception
{
	public bool IsReportingNow { get; private set; }

	protected OwlcatException(Exception innerException)
		: base("", innerException)
	{
	}

	public void Log(LogChannel channel, UnityEngine.Object context = null)
	{
		try
		{
			IsReportingNow = true;
			ReportInternal(channel, context, showReportWindow: false);
		}
		finally
		{
			IsReportingNow = false;
		}
	}

	public void Report(LogChannel channel, UnityEngine.Object context = null)
	{
		try
		{
			IsReportingNow = true;
			ReportInternal(channel, context, showReportWindow: true);
		}
		finally
		{
			IsReportingNow = false;
		}
	}

	protected abstract void ReportInternal(LogChannel channel, [CanBeNull] UnityEngine.Object context, bool showReportWindow);
}
