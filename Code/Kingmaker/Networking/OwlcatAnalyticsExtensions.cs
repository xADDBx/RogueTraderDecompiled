using System.Collections.Generic;
using Core.Cheats;
using Kingmaker.QA.Analytics;

namespace Kingmaker.Networking;

public static class OwlcatAnalyticsExtensions
{
	[Cheat(Name = "send_coop_start")]
	public static void SendCoopStart(string sessionId, int numberOfPlayers)
	{
		OwlcatAnalytics.Instance.SendCoopStart(sessionId, numberOfPlayers);
	}

	public static void SendCoopStart(this OwlcatAnalytics analytics, string sessionId, int numberOfPlayers)
	{
		analytics.CustomEvent("WH_CoopStart", new Dictionary<string, object>
		{
			{ "WH_Coop_NumberOfPlayers", numberOfPlayers },
			{ "WH_Coop_SessionId", sessionId }
		});
	}

	[Cheat(Name = "send_coop_end")]
	public static void SendCoopEnd(string sessionId, int numberOfPlayers)
	{
		OwlcatAnalytics.Instance.SendCoopStart(sessionId, numberOfPlayers);
	}

	public static void SendCoopEnd(this OwlcatAnalytics analytics, string sessionId)
	{
		analytics.CustomEvent("WH_CoopEnd", new Dictionary<string, object> { { "WH_Coop_SessionId", sessionId } });
	}
}
