using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.FactionsReputation;

public interface IHasInputHandler
{
	void AddInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget);
}
