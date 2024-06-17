using System.Collections.Generic;
using Core.Cheats;
using Kingmaker.QA.Analytics;
using UnityEngine;

namespace Kingmaker.Utility;

public static class OwlcatAnalyticsExtensions
{
	[Cheat(Name = "send_crash")]
	public static void SendFatalError()
	{
		OwlcatAnalytics.Instance.SendCrashEvent();
	}

	public static void SendCrashEvent(this OwlcatAnalytics analytics)
	{
		analytics.CustomEvent("WH_Crash", new Dictionary<string, object> { 
		{
			"LatestCrashdump",
			PlayerPrefs.GetString("LatestCrashdump")
		} });
	}
}
