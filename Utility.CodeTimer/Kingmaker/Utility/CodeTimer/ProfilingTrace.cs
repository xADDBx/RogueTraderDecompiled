using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Utility.CodeTimer;

public class ProfilingTrace : IDisposable
{
	private struct TraceEvent
	{
		public long Time;

		public string Name;

		public string Event;
	}

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("CodeTimer");

	private readonly Stopwatch m_Stopwatch;

	private static ProfilingTrace s_CurrentTrace;

	private string m_Name;

	private readonly List<TraceEvent> m_Trace = new List<TraceEvent>();

	private readonly StringBuilder m_StringBuilder = new StringBuilder();

	public static ProfilingTrace? CurrentTrace => s_CurrentTrace;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private ProfilingTrace(string name)
	{
		m_Name = name;
		m_Stopwatch = Stopwatch.StartNew();
		m_StringBuilder.Append("[");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ProfilingTrace Start(string name, bool onlyIf = true)
	{
		if (!BuildModeUtility.IsDevelopment || !onlyIf)
		{
			return null;
		}
		if (CurrentTrace != null)
		{
			Logger.Error("Cannot start trace " + name + ": " + CurrentTrace.m_Name + " already active");
			return null;
		}
		s_CurrentTrace = new ProfilingTrace(name);
		return CurrentTrace;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void End()
	{
		CurrentTrace?.Dispose();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Dispose()
	{
		for (int i = 0; i < m_Trace.Count; i++)
		{
			TraceEvent traceEvent = m_Trace[i];
			m_StringBuilder.Append((i == 0) ? "{" : ",{").Append("\"pid\":1, \"tid\":1, \"ts\":").Append(traceEvent.Time)
				.Append(",\"ph\":\"")
				.Append(traceEvent.Event)
				.Append("\", \"name\":\"")
				.Append(traceEvent.Name)
				.Append("\"}");
		}
		m_StringBuilder.Append("]");
		using (StreamWriter streamWriter = new StreamWriter("Trace_" + m_Name + ".json"))
		{
			streamWriter.WriteLine(m_StringBuilder);
		}
		m_Trace.Clear();
		m_StringBuilder.Clear();
		s_CurrentTrace = null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Event(string text, string type)
	{
		m_Trace.Add(new TraceEvent
		{
			Event = type,
			Name = text,
			Time = m_Stopwatch.ElapsedTicks / 10
		});
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AddBeginEvent(string text)
	{
		Event(text, "B");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AddEndEvent(string text)
	{
		Event(text, "E");
	}

	public void AddSingleEvent(string text)
	{
		Event(text, "i");
	}
}
