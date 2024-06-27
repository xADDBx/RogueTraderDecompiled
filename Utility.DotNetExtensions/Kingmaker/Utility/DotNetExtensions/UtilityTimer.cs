using System;
using System.Diagnostics;

namespace Kingmaker.Utility.DotNetExtensions;

public class UtilityTimer : IDisposable
{
	private readonly string m_Text;

	private readonly Stopwatch m_Stopwatch;

	public UtilityTimer(string text)
	{
		m_Text = text;
		m_Stopwatch = Stopwatch.StartNew();
	}

	public void Dispose()
	{
		m_Stopwatch.Stop();
		PFLog.Default.Log($"Profiled {m_Text}: {m_Stopwatch.ElapsedMilliseconds:0.00}ms");
	}
}
