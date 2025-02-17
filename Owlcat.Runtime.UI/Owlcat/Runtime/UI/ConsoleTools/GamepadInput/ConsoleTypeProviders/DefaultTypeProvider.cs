namespace Owlcat.Runtime.UI.ConsoleTools.GamepadInput.ConsoleTypeProviders;

public class DefaultTypeProvider : ConsoleTypeProvider
{
	public override bool TryGetConsoleType(out ConsoleType type)
	{
		type = ConsoleType.XBox;
		return true;
	}
}
