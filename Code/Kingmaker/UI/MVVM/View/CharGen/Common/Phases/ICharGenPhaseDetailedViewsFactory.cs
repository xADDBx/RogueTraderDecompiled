using Kingmaker.UI.MVVM.VM.CharGen.Phases;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases;

public interface ICharGenPhaseDetailedViewsFactory
{
	ICharGenPhaseDetailedView GetDetailedPhaseView(CharGenPhaseBaseVM viewModel);
}
