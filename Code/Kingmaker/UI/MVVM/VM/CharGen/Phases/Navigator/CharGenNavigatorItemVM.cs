using Kingmaker.UI.MVVM.VM.CharGen.Phases.BackgroundBase;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Navigator;

public class CharGenNavigatorItemVM : CharGenBackgroundBaseItemVM
{
	public CharGenNavigatorItemVM(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
		: base(selectionItem, selectionStateFeature, phaseType)
	{
	}
}
