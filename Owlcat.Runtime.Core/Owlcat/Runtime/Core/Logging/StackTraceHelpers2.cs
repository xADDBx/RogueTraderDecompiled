using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security;
using System.Text;

namespace Owlcat.Runtime.Core.Logging;

public static class StackTraceHelpers2
{
	private static readonly Assembly[] IgnoredAssemblies = new Assembly[0];

	public static bool ShouldShowFrame(StackFrame frame)
	{
		MethodBase method = frame.GetMethod();
		if (method == null)
		{
			return true;
		}
		Assembly assembly = method.DeclaringType?.Assembly;
		if ((object)assembly != null && Array.IndexOf(IgnoredAssemblies, assembly) >= 0)
		{
			return false;
		}
		if (method.IsDefined(typeof(StackTraceHiddenAttribute), inherit: false))
		{
			return false;
		}
		if (method.IsDefined(typeof(StackTraceIgnore), inherit: false))
		{
			return false;
		}
		return true;
	}

	public static string ToStringUnityCompatible(this StackTrace stack)
	{
		StringBuilder stringBuilder = new StringBuilder(255);
		stringBuilder.AppendLine();
		stringBuilder.AppendStackTrace(stack, formatLinks: false);
		return stringBuilder.ToString();
	}

	public static string ToStringText(this StackTrace stack)
	{
		StringBuilder stringBuilder = new StringBuilder(255);
		stringBuilder.AppendLine();
		stringBuilder.AppendStackTrace(stack, formatLinks: false);
		return stringBuilder.ToString();
	}

	private static void AppendStackTrace(this StringBuilder stringBuilder, StackTrace stack, bool formatLinks)
	{
		bool flag = true;
		bool flag2 = true;
		for (int i = 0; i < stack.FrameCount; i++)
		{
			StackFrame frame = stack.GetFrame(i);
			if (!ShouldShowFrame(frame))
			{
				continue;
			}
			MethodBase method = frame.GetMethod();
			if (flag2)
			{
				flag2 = false;
			}
			else
			{
				stringBuilder.Append(Environment.NewLine);
			}
			stringBuilder.Append("   at ");
			if (method == null)
			{
				stringBuilder.Append(frame);
				continue;
			}
			Type declaringType = method.DeclaringType;
			if ((object)declaringType != null)
			{
				string fullName = declaringType.FullName;
				if (fullName != null)
				{
					stringBuilder.Append(fullName.Replace('+', '.'));
					stringBuilder.Append(".");
				}
			}
			stringBuilder.Append(method.Name);
			if (method is MethodInfo && method.IsGenericMethod)
			{
				Type[] genericArguments = method.GetGenericArguments();
				stringBuilder.Append("[");
				int j = 0;
				bool flag3 = true;
				for (; j < genericArguments.Length; j++)
				{
					if (!flag3)
					{
						stringBuilder.Append(",");
					}
					else
					{
						flag3 = false;
					}
					stringBuilder.Append(genericArguments[j].Name);
				}
				stringBuilder.Append("]");
			}
			stringBuilder.Append("(");
			ParameterInfo[] parameters = method.GetParameters();
			bool flag4 = true;
			ParameterInfo[] array = parameters;
			foreach (ParameterInfo parameterInfo in array)
			{
				if (!flag4)
				{
					stringBuilder.Append(", ");
				}
				else
				{
					flag4 = false;
				}
				string name = parameterInfo.ParameterType.Name;
				stringBuilder.Append(name + " " + parameterInfo.Name);
			}
			stringBuilder.Append(")");
			if (flag && frame.GetILOffset() != -1)
			{
				string text = null;
				try
				{
					text = frame.GetFileName();
				}
				catch (NotSupportedException)
				{
					flag = false;
				}
				catch (SecurityException)
				{
					flag = false;
				}
				if (text != null)
				{
					stringBuilder.Append(' ');
					stringBuilder.AppendFormat(CultureInfo.InvariantCulture, formatLinks ? "in <a href=\"{0}\" line=\"{1}\">{0}:{1}</a>" : "in {0}:{1}", text, frame.GetFileLineNumber());
				}
			}
		}
	}

	public static string ToStringUnityCompatible(this Exception e)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine();
		VisitInner(e, stringBuilder, formatLinks: false);
		stringBuilder.AppendLine(" --- End of Exception Stack Trace ---");
		return stringBuilder.ToString();
	}

	public static string ToStringText(this Exception e)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine();
		VisitInner(e, stringBuilder, formatLinks: false);
		stringBuilder.AppendLine(" --- End of Exception Stack Trace ---");
		return stringBuilder.ToString();
	}

	private static void VisitInner(Exception e, StringBuilder sb, bool formatLinks)
	{
		Exception innerException = e.InnerException;
		if (innerException != null)
		{
			VisitInner(innerException, sb, formatLinks);
			sb.AppendLine(" --- Rethrow ---");
		}
		sb.Append(e.GetType());
		if (!string.IsNullOrWhiteSpace(e.Message))
		{
			sb.Append(": ").Append(e.Message);
		}
		sb.AppendLine();
		StackTrace stack = new StackTrace(e, fNeedFileInfo: true);
		sb.AppendStackTrace(stack, formatLinks);
		sb.AppendLine();
	}
}
