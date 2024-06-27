using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Utility.CodeTimer;

public struct CodeTimer : IDisposable
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("CodeTimer");

	private readonly Stopwatch m_Stopwatch;

	private readonly string m_Text;

	private readonly LogChannel m_Channel;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private CodeTimer(Stopwatch sw, string text, LogChannel channel)
	{
		m_Stopwatch = sw;
		m_Text = text;
		m_Channel = channel;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CodeTimer? New(LogChannel channel, string text)
	{
		if (!BuildModeUtility.IsDevelopment)
		{
			return null;
		}
		CodeTimer value = new CodeTimer(new Stopwatch(), text, channel);
		value.m_Stopwatch.Restart();
		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CodeTimer? New(string text)
	{
		return New(Logger, text);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Dispose()
	{
		m_Stopwatch.Stop();
		string messageFormat = $"Profiled {m_Text}: {m_Stopwatch.ElapsedMilliseconds:0.00}ms";
		m_Channel.Log(messageFormat);
	}
}
