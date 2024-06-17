using System;
using System.Collections;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Utility.ManualCoroutines;

public class CoroutineManager
{
	private readonly UpdatableQueue<IEnumerator> m_Running = new UpdatableQueue<IEnumerator>();

	public CoroutineHandler Start(IEnumerator routine, UnityEngine.Object objectHolder = null)
	{
		m_Running.Add(routine);
		return new CoroutineHandler(this, routine, objectHolder);
	}

	public void Stop(IEnumerator routine)
	{
		m_Running.Remove(routine);
	}

	public void Stop(CoroutineHandler routine)
	{
		routine.Stop();
	}

	public void Stop(ref CoroutineHandler routine)
	{
		routine.Stop();
		routine = CoroutineHandler.Empty;
	}

	public void StopAll()
	{
		m_Running.Clear();
	}

	public bool IsRunning(IEnumerator routine)
	{
		return m_Running.Contains(routine);
	}

	public bool IsRunning(CoroutineHandler routine)
	{
		return routine.IsRunning;
	}

	public void Update()
	{
		m_Running.Prepare();
		IEnumerator value;
		while (m_Running.Next(out value))
		{
			bool flag = true;
			try
			{
				flag = value == null || !MoveNext(value);
			}
			catch (Exception ex)
			{
				PFLog.Coroutine.Exception(ex);
			}
			if (flag)
			{
				m_Running.Remove(value);
			}
		}
	}

	private static bool MoveNext(IEnumerator routine)
	{
		if (routine.Current is IEnumerator routine2 && MoveNext(routine2))
		{
			return true;
		}
		if (!routine.MoveNext())
		{
			return false;
		}
		if (routine.Current is IEnumerator routine3)
		{
			MoveNext(routine3);
			return true;
		}
		return true;
	}
}
