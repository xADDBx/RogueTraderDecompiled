using Kingmaker.UI.MVVM.VM.CharGen.Phases.BackgroundBase;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Homeworld.ChildPhases;

public class CharGenImperialHomeworldChildPhaseVM : CharGenBackgroundBasePhaseVM<CharGenBackgroundBaseItemVM>
{
	public CharGenImperialHomeworldChildPhaseVM(CharGenContext charGenContext)
		: base(charGenContext, FeatureGroup.ChargenImperialWorld, CharGenPhaseType.ImperialHomeworldChild, (ReactiveProperty<CharGenPhaseBaseVM>)null)
	{
	}

	protected override CharGenBackgroundBaseItemVM CreateItem(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
	{
		return new CharGenImperialHomeworldItemVM(selectionItem, selectionStateFeature, phaseType);
	}
}
