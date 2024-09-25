using Kingmaker.UI.MVVM.VM.CharGen.Phases.BackgroundBase;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.SoulMark;

public class CharGenSoulMarkItemVM : CharGenBackgroundBaseItemVM
{
	public CharGenSoulMarkItemVM(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
		: base(selectionItem, selectionStateFeature, phaseType)
	{
	}
}
