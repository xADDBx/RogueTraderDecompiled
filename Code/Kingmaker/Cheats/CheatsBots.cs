using System.Linq;
using Core.Cheats;
using Kingmaker.Blueprints;
using Kingmaker.QA.Clockwork;
using UnityEngine;

namespace Kingmaker.Cheats;

internal class CheatsBots
{
	[Cheat(Name = "clockwork_start")]
	public static void ConsoleClockworkStart(string scenario)
	{
		if (Clockwork.IsRunning)
		{
			Clockwork.Instance.Stop();
		}
		Clockwork.Instance.Start(scenario);
	}

	[Cheat(Name = "clockwork_stop")]
	public static void ConsoleClockworkStop()
	{
		if (Clockwork.IsRunning)
		{
			Clockwork.Instance.Stop();
		}
	}

	[Cheat(Name = "clockwork_list_scenarios")]
	public static string ConsoleClockworkScenarioList()
	{
		return $"Available {ClockworkScenarioIndex.Instance.Instructions.Count()} Clockwork scenarios:\n\n" + string.Join("\n", ClockworkScenarioIndex.Instance.Instructions.OrderBy((string x) => x));
	}

	[Cheat(Name = "clockwork_status")]
	public static string ConsoleClockworkStatus()
	{
		string @string = PlayerPrefs.GetString("ClockworkScenario", "");
		if (Clockwork.IsRunning || Clockwork.GameIsLoadingWithScenario)
		{
			if (string.IsNullOrEmpty(@string))
			{
				Clockwork instance = Clockwork.Instance;
				return "running " + ((instance == null) ? null : SimpleBlueprintExtendAsObject.Or(instance.Scenario, null)?.ScenarioName);
			}
			return "starting " + @string;
		}
		if (!Clockwork.Enabled)
		{
			return "Clockwork is not ready or disabled.";
		}
		return "stop";
	}
}
