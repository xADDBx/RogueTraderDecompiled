using System;
using System.Collections;
using Kingmaker.Utility.ManualCoroutines;

namespace Kingmaker.Controllers;

public static class CoroutinesControllerExtensions
{
	public static CoroutineHandler InvokeInTicks(this CoroutinesController coroutinesController, Action action, int ticks)
	{
		int targetTick2 = CurrentTick() + ticks;
		return coroutinesController.Start(Delay(action, targetTick2));
		static int CurrentTick()
		{
			return Game.Instance.RealTimeController.CurrentSystemStepIndex;
		}
		static IEnumerator Delay(Action action, int targetTick)
		{
			while (CurrentTick() < targetTick)
			{
				yield return null;
			}
			action();
		}
	}

	public static CoroutineHandler InvokeInTime(this CoroutinesController coroutinesController, Action action, TimeSpan delay)
	{
		TimeSpan targetTime2 = CurrentTime() + delay;
		return coroutinesController.Start(Delay(action, targetTime2));
		static TimeSpan CurrentTime()
		{
			return Game.Instance.TimeController.RealTime;
		}
		static IEnumerator Delay(Action action, TimeSpan targetTime)
		{
			while (CurrentTime() < targetTime)
			{
				yield return null;
			}
			action();
		}
	}
}
