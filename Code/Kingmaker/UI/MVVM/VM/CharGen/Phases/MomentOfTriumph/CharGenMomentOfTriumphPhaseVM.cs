using Kingmaker.UI.MVVM.VM.CharGen.Phases.BackgroundBase;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.MomentOfTriumph;

public class CharGenMomentOfTriumphPhaseVM : CharGenBackgroundBasePhaseVM<CharGenBackgroundBaseItemVM>
{
	public CharGenMomentOfTriumphPhaseVM(CharGenContext charGenContext)
		: base(charGenContext, FeatureGroup.ChargenMomentOfTriumph, CharGenPhaseType.MomentOfTriumph, (ReactiveProperty<CharGenPhaseBaseVM>)null)
	{
	}

	protected override CharGenBackgroundBaseItemVM CreateItem(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
	{
		return new CharGenMomentOfTriumphItemVM(selectionItem, selectionStateFeature, phaseType);
	}
}
