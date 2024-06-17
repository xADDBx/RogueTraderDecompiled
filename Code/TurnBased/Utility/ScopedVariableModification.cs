using System;
using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;

namespace TurnBased.Utility;

public class ScopedVariableModification<T> : IDisposable
{
	private T m_SavedValue;

	private Action<T> m_RestoreAction;

	private static readonly List<ScopedVariableModification<T>> s_Cache = new List<ScopedVariableModification<T>>();

	private ScopedVariableModification()
	{
	}

	private void Init(T value, Action<T> restoreAction)
	{
		m_SavedValue = value;
		m_RestoreAction = restoreAction;
	}

	public static ScopedVariableModification<T> Create(T value, Action<T> restoreAction)
	{
		ScopedVariableModification<T> scopedVariableModification;
		if (s_Cache.Empty())
		{
			scopedVariableModification = new ScopedVariableModification<T>();
		}
		else
		{
			scopedVariableModification = s_Cache[s_Cache.Count - 1];
			s_Cache.RemoveAt(s_Cache.Count - 1);
		}
		scopedVariableModification.Init(value, restoreAction);
		return scopedVariableModification;
	}

	public void Dispose()
	{
		m_RestoreAction(m_SavedValue);
	}
}
