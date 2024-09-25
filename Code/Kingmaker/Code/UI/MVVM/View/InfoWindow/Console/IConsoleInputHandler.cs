using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.Code.UI.MVVM.View.InfoWindow.Console;

public interface IConsoleInputHandler
{
	void AddInputTo(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, GridConsoleNavigationBehaviour ownerBehaviour);

	void UpdateTooltipBrick();
}
