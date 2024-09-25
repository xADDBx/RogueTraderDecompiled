using Kingmaker.UI.MVVM.VM.CharGen.Phases.BackgroundBase;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Navigator;

public class CharGenNavigatorPhaseVM : CharGenBackgroundBasePhaseVM<CharGenBackgroundBaseItemVM>
{
	public CharGenNavigatorPhaseVM(CharGenContext charGenContext)
		: base(charGenContext, FeatureGroup.ChargenNavigator, CharGenPhaseType.Navigator, (ReactiveProperty<CharGenPhaseBaseVM>)null)
	{
	}

	protected override CharGenBackgroundBaseItemVM CreateItem(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
	{
		return new CharGenNavigatorItemVM(selectionItem, selectionStateFeature, phaseType);
	}
}
