using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Base;

public interface ICharGenAppearancePageComponent : IConsoleNavigationEntity, IConsoleEntity
{
	void AddInput(ref InputLayer inputLayer, ConsoleHintsWidget hintsWidget);

	void RemoveInput();
}
