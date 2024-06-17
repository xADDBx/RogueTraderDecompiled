using System;
using JetBrains.Annotations;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.QA;

public static class LogChannelEx
{
	[StackTraceIgnore]
	[StringFormatMethod("msgFormat")]
	public static void ErrorWithReport(this LogChannel channel, string msgFormat, params object[] @params)
	{
		channel.ErrorWithReport((UnityEngine.Object)null, msgFormat, @params);
	}

	[StackTraceIgnore]
	[StringFormatMethod("msgFormat")]
	public static void ErrorWithReport(this LogChannel channel, UnityEngine.Object ctx, string msgFormat, params object[] @params)
	{
		string message = string.Format(msgFormat, @params);
		channel.Error(ctx, msgFormat, @params);
		QAModeExceptionEvents.Instance.MaybeShowError(message, null, ctx);
	}

	public static void ErrorWithReport(this LogChannel channel, ICanBeLogContext ctx, string msgFormat, params object[] @params)
	{
		string message = string.Format(msgFormat, @params);
		channel.Error(ctx, msgFormat, @params);
		QAModeExceptionEvents.Instance.MaybeShowError(message);
	}

	[StackTraceIgnore]
	[StringFormatMethod("msgFormat")]
	public static void ExceptionWithReport(this LogChannel channel, Exception exception, string msgFormat = null, params object[] @params)
	{
		channel.ExceptionWithReport(null, exception, msgFormat, @params);
	}

	[StackTraceIgnore]
	[StringFormatMethod("msgFormat")]
	public static void ExceptionWithReport(this LogChannel channel, UnityEngine.Object ctx, Exception exception, string msgFormat = null, params object[] @params)
	{
		if (exception is OwlcatException { IsReportingNow: false } ex)
		{
			ex.Report(channel, ctx);
		}
		string message = ((msgFormat != null) ? string.Format(msgFormat, @params) : string.Empty);
		channel.Exception(ctx, exception, msgFormat, @params);
		QAModeExceptionEvents.Instance.MaybeShowError(message, exception, ctx);
	}
}
