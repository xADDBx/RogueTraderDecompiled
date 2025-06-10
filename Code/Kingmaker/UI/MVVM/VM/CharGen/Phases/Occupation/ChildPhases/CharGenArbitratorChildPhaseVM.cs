using Kingmaker.UI.MVVM.VM.CharGen.Phases.BackgroundBase;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Occupation.ChildPhases;

public class CharGenArbitratorChildPhaseVM : CharGenBackgroundBasePhaseVM<CharGenBackgroundBaseItemVM>
{
	public CharGenArbitratorChildPhaseVM(CharGenContext charGenContext)
		: base(charGenContext, FeatureGroup.ChargenArbitrator, CharGenPhaseType.Arbitrator, (ReactiveProperty<CharGenPhaseBaseVM>)null)
	{
	}

	protected override CharGenBackgroundBaseItemVM CreateItem(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
	{
		return new CharGenArbitratorItemVM(selectionItem, selectionStateFeature, phaseType);
	}
}
