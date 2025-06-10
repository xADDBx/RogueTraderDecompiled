using Kingmaker.UI.MVVM.VM.CharGen.Phases.BackgroundBase;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Occupation.ChildPhases;

public class CharGenArbitratorItemVM : CharGenBackgroundBaseItemVM
{
	public CharGenArbitratorItemVM(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
		: base(selectionItem, selectionStateFeature, phaseType)
	{
	}
}
