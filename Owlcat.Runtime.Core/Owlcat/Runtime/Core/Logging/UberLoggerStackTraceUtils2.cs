using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Owlcat.Runtime.Core.Logging;

public static class UberLoggerStackTraceUtils2
{
	public static event Func<StackFrame, bool> CanIgnoreMethodInStacktrace;

	private static bool IgnoreStackTrace(StackFrame stackFrame)
	{
		if (UberLoggerStackTraceUtils2.CanIgnoreMethodInStacktrace != null)
		{
			return UberLoggerStackTraceUtils2.CanIgnoreMethodInStacktrace(stackFrame);
		}
		return false;
	}

	[StackTraceIgnore]
	public static void GetCallstack(List<LogStackFrame> callstack, StackTrace stackTrace)
	{
		IEnumerable<LogStackFrame> collection = from x in stackTrace.GetFrames() ?? Array.Empty<StackFrame>()
			where !IgnoreStackTrace(x)
			select x into stackFrame
			select new
			{
				stackFrame = stackFrame,
				method = stackFrame.GetMethod()
			} into t
			where t.method != null && !t.method.IsDefined(typeof(StackTraceIgnore), inherit: true)
			select new LogStackFrame(t.stackFrame);
		callstack.AddRange(collection);
	}

	public static void FormatStack(StringBuilder stringBuilder, List<LogStackFrame> frames, bool formatLinks)
	{
		bool flag = true;
		foreach (LogStackFrame frame in frames)
		{
			if (!flag)
			{
				stringBuilder.AppendLine();
			}
			else
			{
				flag = false;
			}
			if (string.IsNullOrEmpty(frame.DeclaringType) && string.IsNullOrEmpty(frame.MethodName) && string.IsNullOrEmpty(frame.ParameterSig))
			{
				stringBuilder.AppendLine(frame.GetFormattedMethodName());
				continue;
			}
			stringBuilder.AppendFormat("   at {0}.{1}({2})", frame.DeclaringType, frame.MethodName, frame.ParameterSig);
			string fileName = frame.FileName;
			if (!string.IsNullOrWhiteSpace(fileName))
			{
				stringBuilder.Append(' ');
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, formatLinks ? "in <a href=\"{0}\" line=\"{1}\">{0}:{1}</a>" : "in {0}:{1}", fileName, frame.LineNumber);
			}
		}
	}

	public static string StackToString(List<LogStackFrame> frames, bool formatLinks)
	{
		StringBuilder stringBuilder = new StringBuilder();
		FormatStack(stringBuilder, frames, formatLinks);
		return stringBuilder.ToString();
	}
}
