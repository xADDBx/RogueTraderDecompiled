using System;
using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Core.Logging;

public class AddContextToUnityLog : IDisposable
{
	[ThreadStatic]
	private static Stack<AddContextToUnityLog> s_Pool = new Stack<AddContextToUnityLog>();

	private string m_Context;

	private LogChannel m_Channel;

	private bool m_IncludeLog;

	public static AddContextToUnityLog New(string context, LogChannel channel = null, bool includeLog = false)
	{
		s_Pool = s_Pool ?? new Stack<AddContextToUnityLog>();
		AddContextToUnityLog obj = ((s_Pool.Count > 0) ? s_Pool.Pop() : new AddContextToUnityLog());
		obj.m_Context = context;
		obj.m_Channel = channel ?? LogChannel.Default;
		obj.m_IncludeLog = includeLog;
		Application.logMessageReceived += obj.OnLog;
		return obj;
	}

	private AddContextToUnityLog()
	{
	}

	private void OnLog(string condition, string stacktrace, LogType type)
	{
		if (type != LogType.Log || m_IncludeLog)
		{
			m_Channel.Log("Context: {0}", m_Context);
		}
	}

	public void Dispose()
	{
		Application.logMessageReceived -= OnLog;
		s_Pool.Push(this);
	}
}
