using Kingmaker.UI.MVVM.VM.CharGen.Phases.BackgroundBase;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Homeworld;

public class CharGenHomeworldItemVM : CharGenBackgroundBaseItemVM
{
	public CharGenHomeworldItemVM(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
		: base(selectionItem, selectionStateFeature, phaseType)
	{
	}
}
