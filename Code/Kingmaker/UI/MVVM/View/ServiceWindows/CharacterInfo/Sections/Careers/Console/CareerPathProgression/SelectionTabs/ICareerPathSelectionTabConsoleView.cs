using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.SelectionTabs;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Console.CareerPathProgression.SelectionTabs;

public interface ICareerPathSelectionTabConsoleView : ICareerPathSelectionTabView
{
	void AddInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget);
}
