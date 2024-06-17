using System;
using System.Collections;
using System.Collections.Generic;
using Code.Package.Runtime.Extensions.Dependencies;
using UniRx;
using UnityEngine;

namespace Owlcat.Runtime.UniRx;

public static class DelayedInvoker
{
	private static List<Action> s_ActionsInFrame = new List<Action>();

	private static bool s_InvokeActionsAtTheEndOfFrameCoroutineIsRunning = false;

	public static IDisposable InvokeInFrames(Action action, int frames)
	{
		Coroutine coroutine = DelayedInvokerCoroutineRunner.Start(InvokeInFramesCoroutine(action, frames));
		return Disposable.Create(delegate
		{
			DelayedInvokerCoroutineRunner.Stop(coroutine);
		});
	}

	public static IDisposable InvokeInTime(Action action, float time, bool realtime = true)
	{
		Coroutine coroutine = DelayedInvokerCoroutineRunner.Start(InvokeInTimeCoroutine(action, time, realtime));
		return Disposable.Create(delegate
		{
			DelayedInvokerCoroutineRunner.Stop(coroutine);
		});
	}

	public static IDisposable InvokeWhenTrue(Action action, Func<bool> predicate)
	{
		Coroutine coroutine = DelayedInvokerCoroutineRunner.Start(InvokeWhenTrueCoroutine(action, predicate));
		return Disposable.Create(delegate
		{
			DelayedInvokerCoroutineRunner.Stop(coroutine);
		});
	}

	public static void InvokeAtTheEndOfFrameOnlyOnes(Action action)
	{
		if (!s_InvokeActionsAtTheEndOfFrameCoroutineIsRunning)
		{
			DelayedInvokerCoroutineRunner.Start(InvokeActionsAtTheEndOfFrameCoroutine());
			s_InvokeActionsAtTheEndOfFrameCoroutineIsRunning = true;
		}
		if (!s_ActionsInFrame.Contains(action))
		{
			s_ActionsInFrame.Add(action);
		}
	}

	private static IEnumerator InvokeInFramesCoroutine(Action action, int frames)
	{
		for (int i = 0; i < frames; i++)
		{
			yield return null;
		}
		action?.Invoke();
	}

	private static IEnumerator InvokeInTimeCoroutine(Action action, float time, bool realtime)
	{
		if (realtime)
		{
			yield return new WaitForSecondsRealtime(time);
		}
		else
		{
			yield return new WaitForSeconds(time);
		}
		action?.Invoke();
	}

	private static IEnumerator InvokeWhenTrueCoroutine(Action action, Func<bool> predicate)
	{
		while (!(predicate?.Invoke() ?? false))
		{
			yield return null;
		}
		action?.Invoke();
	}

	private static IEnumerator InvokeActionsAtTheEndOfFrameCoroutine()
	{
		while (true)
		{
			if (s_ActionsInFrame.Count == 0)
			{
				yield return null;
				continue;
			}
			foreach (Action item in s_ActionsInFrame)
			{
				try
				{
					item?.Invoke();
				}
				catch (Exception message)
				{
					UniRxLogger.Exception(message);
				}
			}
			s_ActionsInFrame.Clear();
			yield return null;
		}
	}
}
