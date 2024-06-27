using System;
using System.Runtime.CompilerServices;
using Kingmaker.Utility.BuildModeUtils;

namespace Kingmaker.Utility.CodeTimer;

public struct CodeTimerTraceScope : IDisposable
{
	private string m_Name;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private CodeTimerTraceScope(string name)
	{
		m_Name = name;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CodeTimerTraceScope? New(string text)
	{
		if (!BuildModeUtility.IsDevelopment || ProfilingTrace.CurrentTrace == null)
		{
			return null;
		}
		ProfilingTrace.CurrentTrace.AddBeginEvent(text);
		return new CodeTimerTraceScope(text);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Dispose()
	{
		ProfilingTrace.CurrentTrace.AddEndEvent(m_Name);
	}
}
