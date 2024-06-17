using System;
using System.Collections.Generic;
using Core.Cheats;
using Kingmaker.GameModes;
using Kingmaker.QA.Analytics;
using UnityEngine;

namespace Kingmaker;

public static class OwlcatAnalyticsExtensions
{
	[Cheat(Name = "send_fatal_error")]
	public static void SendFatalError(string message)
	{
		OwlcatAnalytics.Instance.SendFatalError(message);
	}

	public static void SendFatalError(this OwlcatAnalytics analytics, string message)
	{
		analytics.CustomEvent("WH_FatalError", new Dictionary<string, object> { { "Message", message } });
	}

	public static void SendStartGameSession(this OwlcatAnalytics analytics)
	{
		analytics.CustomEvent("WH_StartGameSession", null);
	}

	public static void SendEndGameSession(this OwlcatAnalytics analytics)
	{
		analytics.CustomEvent("WH_EndGameSession", null);
	}

	public static void SendRamInsufficiency(this OwlcatAnalytics analytics, long memoryRedLimit, long memory, GameModeType instanceCurrentMode, string currentArea, Vector3 partyPosition, TimeSpan currentAreaEnterTime)
	{
		analytics.CustomEvent("WH_RAM_INSUFFICIENCY", new Dictionary<string, object>
		{
			{ "Threshold", memoryRedLimit },
			{ "Memory", memory },
			{
				"State",
				instanceCurrentMode.ToString()
			},
			{ "Area", currentArea },
			{
				"Position",
				partyPosition.ToString()
			},
			{
				"TimeFromEnter",
				currentAreaEnterTime.ToString()
			}
		});
	}
}
