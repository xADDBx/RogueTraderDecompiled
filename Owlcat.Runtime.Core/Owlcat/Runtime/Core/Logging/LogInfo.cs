using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using UnityEngine;

namespace Owlcat.Runtime.Core.Logging;

[Serializable]
public class LogInfo
{
	private static readonly Regex ErrorRegex = new Regex("error CS[0-9]+:");

	[CanBeNull]
	public object Source;

	[NotNull]
	public LogChannel Channel;

	public LogSeverity Severity;

	[NotNull]
	public string Message;

	[CanBeNull]
	public List<LogStackFrame> Callstack;

	[SerializeField]
	public DateTime TimeStamp;

	[SerializeField]
	private string m_TimeStampAsString;

	[SerializeField]
	private bool m_SingleStringConstructed;

	[SerializeField]
	private string m_SingleString;

	[SerializeField]
	private bool m_SingleStringWithTimeConstructed;

	[SerializeField]
	private string m_SingleStringWithTime;

	[SerializeField]
	private bool m_SingleStringWithTimeExtendedConstructed;

	[SerializeField]
	private string m_SingleStringWithTimeExtended;

	[SerializeField]
	public bool IsCompilationError;

	public string GetTimeStampAsString()
	{
		return m_TimeStampAsString;
	}

	public string GetFormattedMessage()
	{
		return "[" + GetTimeStampAsString() + "]: " + Message;
	}

	public LogInfo([CanBeNull] object source, [NotNull] LogChannel channel, DateTime timeStamp, LogSeverity severity, [CanBeNull] List<LogStackFrame> callstack, string message)
	{
		Source = source;
		Channel = channel;
		Severity = severity;
		Message = message;
		Callstack = callstack;
		TimeStamp = timeStamp;
		m_TimeStampAsString = timeStamp.ToString("dd.MM.yyyy HH:mm:ss:fff");
		IsCompilationError = Severity == LogSeverity.Error && ErrorRegex.IsMatch(Message);
	}

	public string GetSingleString()
	{
		if (m_SingleStringConstructed)
		{
			return m_SingleString;
		}
		m_SingleString = Message.Replace(Logger.UnityInternalNewLine, " ");
		if (string.IsNullOrWhiteSpace(m_SingleString) && Callstack != null && Callstack.Count > 0)
		{
			m_SingleString = Callstack[0].GetFormattedMethodName().Split(Logger.UnityInternalNewLine)[0];
		}
		m_SingleStringConstructed = true;
		return m_SingleString;
	}

	public string GetSingleString(bool withTime)
	{
		if (!withTime)
		{
			return GetSingleString();
		}
		if (m_SingleStringWithTimeConstructed)
		{
			return m_SingleStringWithTime;
		}
		m_SingleStringWithTime = m_TimeStampAsString + ": " + GetSingleString();
		m_SingleStringWithTimeConstructed = true;
		return m_SingleStringWithTime;
	}
}
