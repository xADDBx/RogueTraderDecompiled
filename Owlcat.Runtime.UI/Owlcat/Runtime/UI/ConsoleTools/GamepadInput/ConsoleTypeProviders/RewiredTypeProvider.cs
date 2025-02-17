using Rewired;

namespace Owlcat.Runtime.UI.ConsoleTools.GamepadInput.ConsoleTypeProviders;

public class RewiredTypeProvider : ConsoleTypeProvider
{
	private const string RewiredDualShock4Name = "Sony DualShock 4";

	private const string RewiredDualSense5Name = "Sony DualSense";

	private const string RewiredNintendoSwitchName = "Nintendo Switch Pro Controller";

	private const string RewiredSteamControllerName = "Steam Controller";

	public override bool TryGetConsoleType(out ConsoleType type)
	{
		Joystick[] array = ReInput.controllers?.GetJoysticks();
		if (array == null || array.Length == 0 || array[0] == null)
		{
			type = ConsoleType.Common;
			return false;
		}
		switch (array[0].name)
		{
		case "Sony DualShock 4":
			type = ConsoleType.PS4;
			break;
		case "Sony DualSense":
			type = ConsoleType.PS5;
			break;
		case "Nintendo Switch Pro Controller":
			type = ConsoleType.Switch;
			break;
		case "Steam Controller":
			type = ConsoleType.SteamController;
			break;
		default:
			type = ConsoleType.Common;
			return false;
		}
		return true;
	}
}
