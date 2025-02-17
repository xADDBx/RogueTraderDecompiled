namespace Owlcat.Runtime.UI.ConsoleTools.GamepadInput.ConsoleTypeProviders;

public class PlatformTypeProvider : ConsoleTypeProvider
{
	public override bool TryGetConsoleType(out ConsoleType type)
	{
		type = ConsoleType.Common;
		return false;
	}
}
