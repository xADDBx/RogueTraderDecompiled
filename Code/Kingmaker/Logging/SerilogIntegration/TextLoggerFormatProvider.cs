using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Logging.SerilogIntegration;

public class TextLoggerFormatProvider : IFormatProvider, ICustomFormatter
{
	public static readonly TextLoggerFormatProvider Instance = new TextLoggerFormatProvider();

	private TextLoggerFormatProvider()
	{
	}

	public object GetFormat(Type formatType)
	{
		if (!formatType.IsInstanceOfType(this))
		{
			return null;
		}
		return this;
	}

	public string Format(string format, object arg, IFormatProvider formatProvider)
	{
		if (!(arg is StackTrace stack))
		{
			if (!(arg is Exception e))
			{
				if (arg is List<LogStackFrame> frames)
				{
					return FormatStack(frames, formatLinks: false);
				}
				return DefaultFormat(format, arg);
			}
			return e.ToStringText();
		}
		return stack.ToStringText();
	}

	private static string DefaultFormat(string format, object arg)
	{
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		return ((ICustomFormatter)invariantCulture.GetFormat(typeof(ICustomFormatter)))?.Format(format, arg, invariantCulture) ?? arg.ToString();
	}

	public static string FormatStack(List<LogStackFrame> frames, bool formatLinks)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine();
		foreach (LogStackFrame frame in frames)
		{
			if (string.IsNullOrEmpty(frame.DeclaringType) && string.IsNullOrEmpty(frame.MethodName) && string.IsNullOrEmpty(frame.ParameterSig))
			{
				stringBuilder.AppendLine(frame.GetFormattedMethodName());
				continue;
			}
			stringBuilder.AppendFormat("   at {0}.{1}({2})", frame.DeclaringType, frame.MethodName, frame.ParameterSig);
			if (!string.IsNullOrWhiteSpace(frame.FileName))
			{
				stringBuilder.Append(' ');
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, formatLinks ? "in <a href=\"{0}\" line=\"{1}\">{0}:{1}</a>" : "in {0}:{1}", frame.FileName, frame.LineNumber);
			}
			stringBuilder.AppendLine();
		}
		return stringBuilder.ToString();
	}
}
