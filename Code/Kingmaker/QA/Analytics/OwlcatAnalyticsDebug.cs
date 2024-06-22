using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Cheats;
using UnityEngine;

namespace Kingmaker.QA.Analytics;

public static class OwlcatAnalyticsDebug
{
	private static int DebugCreateEventsCounter;

	private static int DebugCreateEventsInterval;

	[Cheat(Name = "debug_analytics_create_events", Description = "Создать кучу эвентов в аналитику", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static async Task DebugCreateEvents(int count, int interval)
	{
		if (DebugCreateEventsCounter > 0)
		{
			DebugCreateEventsCounter = count;
			DebugCreateEventsInterval = interval;
		}
		else
		{
			DebugCreateEventsCounter = count;
			DebugCreateEventsInterval = interval;
			Generate();
		}
	}

	private static async Task Generate()
	{
		try
		{
			while ((DebugCreateEventsCounter > 0 || Application.exitCancellationToken.IsCancellationRequested) && DebugCreateEventsInterval != 0)
			{
				OwlcatAnalytics.Instance.CustomEvent("DebugEvent", new Dictionary<string, object> { { "remaining", DebugCreateEventsCounter } });
				DebugCreateEventsCounter--;
				await Task.Delay(DebugCreateEventsInterval);
			}
		}
		catch (Exception)
		{
			DebugCreateEventsCounter = 0;
			DebugCreateEventsInterval = 0;
		}
	}
}
