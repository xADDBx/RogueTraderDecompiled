namespace Owlcat.Runtime.UI.ConsoleTools.GamepadInput.ConsoleTypeProviders;

public abstract class ConsoleTypeProvider
{
	public abstract bool TryGetConsoleType(out ConsoleType type);
}
