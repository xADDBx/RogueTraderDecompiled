using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Owlcat.QA.Validation;

public static class ValidationProgressCollection
{
	private static readonly Dictionary<int, ValidationProgress> m_Progresses = new Dictionary<int, ValidationProgress>();

	private static int m_Seq;

	public static int StartNew(string name, int total = 1)
	{
		ValidationProgress value = new ValidationProgress(Interlocked.Increment(ref m_Seq), name, total);
		m_Progresses.Add(m_Seq, value);
		return m_Seq;
	}

	public static void SetTotal(int id, int total)
	{
		if (m_Progresses.TryGetValue(id, out var value) && value.IsRun)
		{
			value.SetTotal(total);
		}
	}

	public static void Step(int id, string name)
	{
		if (m_Progresses.TryGetValue(id, out var value) && value.IsRun)
		{
			value.Step(name);
		}
	}

	public static void Stop(int id, ValidationResult result = ValidationResult.Success)
	{
		if (m_Progresses.TryGetValue(id, out var value) && value.IsRun)
		{
			value.Stop(result);
		}
	}

	public static void RegisterValueChangedCallBack(int id, Action callback)
	{
		if (m_Progresses.TryGetValue(id, out var value))
		{
			ValidationProgress validationProgress = value;
			validationProgress.Stepped = (Action)Delegate.Combine(validationProgress.Stepped, callback);
		}
	}

	public static void RegisterStopedCallBack(int id, Action<ValidationResult> callback)
	{
		if (m_Progresses.TryGetValue(id, out var value))
		{
			ValidationProgress validationProgress = value;
			validationProgress.Stopped = (Action<ValidationResult>)Delegate.Combine(validationProgress.Stopped, callback);
		}
	}

	public static void UnregisterValueChangedCallBacks(int id)
	{
		if (m_Progresses.TryGetValue(id, out var value))
		{
			value.Stepped = null;
		}
	}

	public static void UnregisterStopedCallBacks(int id)
	{
		if (m_Progresses.TryGetValue(id, out var value))
		{
			value.Stopped = null;
		}
	}

	public static bool IsExists(int id)
	{
		if (m_Progresses.TryGetValue(id, out var value))
		{
			return value.IsRun;
		}
		return false;
	}

	public static float GetProgressTime(int id)
	{
		if (m_Progresses.TryGetValue(id, out var value))
		{
			return value.TimeMs;
		}
		return 0f;
	}

	public static void ClearInactive()
	{
		int[] array = m_Progresses.Keys.ToArray();
		foreach (int key in array)
		{
			if (!m_Progresses[key].IsRun)
			{
				m_Progresses.Remove(key);
			}
		}
	}

	public static void ClearAll()
	{
		m_Progresses.Clear();
	}
}
