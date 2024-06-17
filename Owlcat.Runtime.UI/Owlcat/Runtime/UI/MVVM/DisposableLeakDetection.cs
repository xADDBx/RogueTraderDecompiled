using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace Owlcat.Runtime.UI.MVVM;

internal static class DisposableLeakDetection
{
	private readonly struct Entry
	{
		public readonly StackTrace CreatedCallstack;

		public readonly string TypeName;

		public readonly int OwnerHashCode;

		public Entry(StackTrace createdCallstack, string typeName, int ownerHashCode)
		{
			CreatedCallstack = createdCallstack;
			TypeName = typeName;
			OwnerHashCode = ownerHashCode;
		}
	}

	private class Wrapper : IDisposable
	{
		private IDisposable m_Wrapped;

		public Wrapper(IDisposable wrapped)
		{
			m_Wrapped = wrapped;
		}

		public void Dispose()
		{
			m_Wrapped.Dispose();
		}
	}

	private static readonly Dictionary<int, Entry> Store = new Dictionary<int, Entry>();

	[Conditional("DEBUG_DISPOSABLES")]
	public static void OnCreated(object d)
	{
		int hashCode = d.GetHashCode();
		Store[hashCode] = new Entry(new StackTrace(2), d.GetType().Name, 0);
	}

	[Conditional("DEBUG_DISPOSABLES")]
	public static void OnDisposed(object d)
	{
		Store.Remove(d.GetHashCode());
	}

	[Conditional("DEBUG_DISPOSABLES")]
	public static void SetOwner(object d, object owner)
	{
		if (d != null)
		{
			int hashCode = d.GetHashCode();
			if (Store.TryGetValue(hashCode, out var value))
			{
				Store[hashCode] = new Entry(value.CreatedCallstack, value.TypeName, owner.GetHashCode());
			}
		}
	}

	[Conditional("DEBUG_DISPOSABLES")]
	public static void ClearOwner(object d)
	{
		if (d != null)
		{
			int hashCode = d.GetHashCode();
			if (Store.TryGetValue(hashCode, out var value))
			{
				Store[hashCode] = new Entry(value.CreatedCallstack, value.TypeName, 0);
			}
		}
	}

	[Conditional("DEBUG_DISPOSABLES")]
	public static void Dump(ILogger logger)
	{
		logger.Log($"Total disposables tracked: {Store.Count}");
		foreach (IGrouping<string, Entry> item in from e in Store.Values
			where !Store.ContainsKey(e.OwnerHashCode)
			group e by e.TypeName)
		{
			logger.Log($"\r\n============================================================================\r\n        {item.Key}: {item.Count()} total instances\r\n============================================================================");
			foreach (IGrouping<string, Entry> item2 in from g in item
				group g by g.CreatedCallstack.ToString())
			{
				logger.Log($"{item2.Count()} instances created\n{item2.Key}");
			}
		}
	}

	public static IDisposable Wrap(IDisposable d)
	{
		return d;
	}
}
