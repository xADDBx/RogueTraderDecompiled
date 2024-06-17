using Owlcat.Runtime.UI.ConsoleTools;

namespace Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;

public interface IConsoleTooltipBrick
{
	bool IsBinded { get; }

	IConsoleEntity GetConsoleEntity();
}
