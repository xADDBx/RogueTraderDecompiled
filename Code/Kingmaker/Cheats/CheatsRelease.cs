using Kingmaker.Blueprints;
using Kingmaker.UI.InputSystems;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Cheats;

internal class CheatsRelease
{
	public static void RegisterCheats(KeyboardAccess keyboard)
	{
		CheatsJira.RegisterCommands(keyboard);
		if (BuildModeUtility.CheatsEnabled)
		{
			SmartConsole.RegisterCommand("time_scale", TimeScale);
			SmartConsole.RegisterCommand("unlock_flag", UnlockFlag);
		}
	}

	public static void TimeScale(string parameters)
	{
		float? paramFloat = Utilities.GetParamFloat(parameters, 1, "Cant parse scale factor from string '" + parameters + "'");
		if (paramFloat.HasValue)
		{
			Game.Instance.TimeController.DebugTimeScale = paramFloat.Value;
			LogChannel smartConsole = PFLog.SmartConsole;
			float? num = paramFloat;
			smartConsole.Log("Time scale changed to " + num);
		}
	}

	private static void UnlockFlag(string parameters)
	{
		string paramString = Utilities.GetParamString(parameters, 1, "Can't parse flag name from given parameters");
		BlueprintUnlockableFlag blueprint = Utilities.GetBlueprint<BlueprintUnlockableFlag>(paramString);
		if (blueprint == null)
		{
			PFLog.SmartConsole.Log("Can't find Flag with name: " + paramString);
			return;
		}
		int? paramInt = Utilities.GetParamInt(parameters, 2, "Can't parse value name from given parameters");
		Game.Instance.Player.UnlockableFlags.Unlock(blueprint);
		Game.Instance.Player.UnlockableFlags.SetFlagValue(blueprint, paramInt.GetValueOrDefault());
		PFLog.SmartConsole.Log("Flag " + paramString + " unlocked with value " + (paramInt?.ToString() ?? "0"));
	}
}
