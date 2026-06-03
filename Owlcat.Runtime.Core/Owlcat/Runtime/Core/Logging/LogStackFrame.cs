using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Owlcat.Runtime.Core.Logging;

[Serializable]
public class LogStackFrame
{
	public static IStackTraceFormatter StackTraceFormatter;

	public string MethodName;

	public string DeclaringType;

	public string ParameterSig;

	public int LineNumber;

	public string FileName;

	private string m_FormattedMethodName;

	public LogStackFrame(StackFrame frame)
	{
		MethodBase method = frame.GetMethod();
		MethodName = GetMethodName(method);
		DeclaringType = ((method.DeclaringType == null) ? "<None>" : GetTypeName(method.DeclaringType));
		ParameterInfo[] parameters = method.GetParameters();
		ParameterSig = GetParameterNames(parameters);
		FileName = frame.GetFileName();
		LineNumber = frame.GetFileLineNumber();
		m_FormattedMethodName = MakeFormattedMethodName();
	}

	public LogStackFrame(string unityStackFrame)
	{
		if (Logger.ExtractInfoFromUnityStackInfo(unityStackFrame, out DeclaringType, out MethodName, out FileName, out LineNumber))
		{
			m_FormattedMethodName = MakeFormattedMethodName();
		}
		else if (!string.IsNullOrWhiteSpace(unityStackFrame))
		{
			m_FormattedMethodName = "   at " + unityStackFrame;
		}
		else
		{
			m_FormattedMethodName = "";
		}
	}

	public LogStackFrame(string message, string filename, int lineNumber)
	{
		FileName = filename;
		LineNumber = lineNumber;
		m_FormattedMethodName = message;
	}

	private static string GetTypeName(Type type)
	{
		if (StackTraceFormatter != null)
		{
			return StackTraceFormatter.ToString(type);
		}
		return type.FullName;
	}

	private static string GetMethodName(MethodBase method)
	{
		if (StackTraceFormatter != null)
		{
			return StackTraceFormatter.ToString(method);
		}
		return method.Name;
	}

	private static string GetParameterNames(ParameterInfo[] parameterInfos)
	{
		if (StackTraceFormatter != null)
		{
			return StackTraceFormatter.ToString(parameterInfos);
		}
		return string.Join(",", parameterInfos.Select((ParameterInfo info) => info.ParameterType.ToString()).ToArray());
	}

	public string GetFormattedMethodName()
	{
		return m_FormattedMethodName;
	}

	private string MakeFormattedMethodName()
	{
		string text = FileName;
		if (!string.IsNullOrEmpty(FileName))
		{
			int num = FileName.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
			if (num >= 0)
			{
				text = FileName.Substring(num);
			}
		}
		string text2 = null;
		if (StackTraceFormatter != null)
		{
			text2 = StackTraceFormatter.Format(DeclaringType, MethodName, ParameterSig, text, LineNumber);
		}
		if (text2 == null)
		{
			text2 = $"   at {DeclaringType}.{MethodName}({ParameterSig}) (in {text}:{LineNumber})";
		}
		return text2;
	}

	public override int GetHashCode()
	{
		return (((LineNumber * 397) ^ ((FileName != null) ? FileName.GetHashCode() : 0)) * 397) ^ ((m_FormattedMethodName != null) ? m_FormattedMethodName.GetHashCode() : 0);
	}
}
