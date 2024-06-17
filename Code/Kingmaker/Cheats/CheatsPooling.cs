using Kingmaker.UI.InputSystems;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Visual.Particles.GameObjectsPooling;

namespace Kingmaker.Cheats;

public class CheatsPooling
{
	public static void RegisterCommands(KeyboardAccess keyboard)
	{
		if (BuildModeUtility.CheatsEnabled)
		{
			SmartConsole.RegisterCommand("pooling_disable", "Disable FX pooling", DisablePool);
			SmartConsole.RegisterCommand("pooling_enable", "Enable FX pooling", EnablePool);
			SmartConsole.RegisterCommand("pooling_reset", "Reset FX pool", ResetPool);
		}
	}

	public static void DisablePool(string parameters)
	{
		GameObjectsPool.ResetAllPools();
		GameObjectsPool.Disabled = true;
	}

	public static void EnablePool(string parameters)
	{
		GameObjectsPool.Disabled = false;
	}

	public static void ResetPool(string parameters)
	{
		GameObjectsPool.ResetAllPools();
	}
}
