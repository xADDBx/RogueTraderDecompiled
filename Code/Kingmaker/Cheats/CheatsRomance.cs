using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Utility.BuildModeUtils;

namespace Kingmaker.Cheats;

internal class CheatsRomance
{
	public static void RegisterCheats()
	{
		if (BuildModeUtility.CheatsEnabled)
		{
			SmartConsole.RegisterCommand("romance_info", RomanceInfo);
		}
	}

	private static void RomanceInfo(string parameters)
	{
		foreach (BlueprintRomanceCounter scriptableObject in Utilities.GetScriptableObjects<BlueprintRomanceCounter>())
		{
			PFLog.SmartConsole.Log("Romance " + Utilities.GetBlueprintName(scriptableObject));
			PFLog.SmartConsole.Log(FlagInfo(scriptableObject.CounterFlag));
			PFLog.SmartConsole.Log(FlagInfo(scriptableObject.MaxValueFlag));
			PFLog.SmartConsole.Log(FlagInfo(scriptableObject.CounterFlag));
		}
	}

	private static string FlagInfo(BlueprintUnlockableFlag counterCounterFlag)
	{
		Dictionary<BlueprintUnlockableFlag, int> unlockedFlags = Game.Instance.Player.UnlockableFlags.UnlockedFlags;
		if (unlockedFlags.ContainsKey(counterCounterFlag))
		{
			return Utilities.GetBlueprintName(counterCounterFlag) + unlockedFlags[counterCounterFlag];
		}
		return Utilities.GetBlueprintName(counterCounterFlag) + "Absent";
	}
}
