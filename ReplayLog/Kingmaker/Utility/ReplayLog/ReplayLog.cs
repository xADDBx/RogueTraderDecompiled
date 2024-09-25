using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Kingmaker.Utility.ReplayLog;

public static class ReplayLog
{
	public const bool Enabled = false;

	[Conditional("FALSE")]
	public static void Add(string text, bool stacktrace = false)
	{
	}

	[Conditional("FALSE")]
	public static void AddIf(bool condition, string text, bool stacktrace = false)
	{
	}

	[Conditional("FALSE")]
	public static void AddForeach<T>(IEnumerable<T> container, Func<T, string> func)
	{
	}

	[Conditional("FALSE")]
	public static void AddForeach<T>(Func<IEnumerable<T>> containerCreator, Func<T, string> func)
	{
	}

	[Conditional("FALSE")]
	public static void StackTrace()
	{
	}

	[Conditional("FALSE")]
	public static void Tick(TimeSpan realTime, int tick)
	{
	}

	[Conditional("FALSE")]
	public static void Clear()
	{
	}

	[Conditional("FALSE")]
	public static void SaveToFile(string replayLogPath)
	{
	}
}
