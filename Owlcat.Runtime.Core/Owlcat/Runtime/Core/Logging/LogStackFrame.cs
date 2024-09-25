using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Owlcat.Runtime.Core.Logging;

[Serializable]
public class LogStackFrame
{
	public string MethodName;

	public string DeclaringType;

	public string ParameterSig;

	public int LineNumber;

	public string FileName;

	private string m_FormattedMethodName;

	public LogStackFrame(StackFrame frame)
	{
		MethodBase method = frame.GetMethod();
		MethodName = method.Name;
		DeclaringType = ((method.DeclaringType == null) ? "<None>" : method.DeclaringType.FullName);
		ParameterInfo[] parameters = method.GetParameters();
		ParameterSig = string.Join(",", parameters.Select((ParameterInfo p) => p.ParameterType.ToString()).ToArray());
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
		return $"   at {DeclaringType}.{MethodName}({ParameterSig}) (in {text}:{LineNumber})";
	}

	public override int GetHashCode()
	{
		return (((LineNumber * 397) ^ ((FileName != null) ? FileName.GetHashCode() : 0)) * 397) ^ ((m_FormattedMethodName != null) ? m_FormattedMethodName.GetHashCode() : 0);
	}
}
