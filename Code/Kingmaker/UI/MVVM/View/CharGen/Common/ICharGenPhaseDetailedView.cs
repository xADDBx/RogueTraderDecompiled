using Kingmaker.Code.UI.MVVM;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UniRx;

namespace Kingmaker.UI.MVVM.View.CharGen.Common;

public interface ICharGenPhaseDetailedView : IInitializable
{
	bool HasYScrollBind { get; }

	bool PressConfirmOnPhase();

	bool PressDeclineOnPhase();

	void Unbind();

	void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget, BoolReactiveProperty isMainCharacter);

	IReadOnlyReactiveProperty<bool> GetCanGoNextOnConfirmProperty();

	IReadOnlyReactiveProperty<bool> CanGoNextInMenuProperty();

	IReadOnlyReactiveProperty<bool> GetCanGoBackOnDeclineProperty();
}
