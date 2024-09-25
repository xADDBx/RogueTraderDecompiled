using Kingmaker.UI.MVVM.VM.CharGen.Phases.BackgroundBase;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Occupation.ChildPhases;

public class CharGenSanctionedPsykerChildPhaseVM : CharGenBackgroundBasePhaseVM<CharGenBackgroundBaseItemVM>
{
	public CharGenSanctionedPsykerChildPhaseVM(CharGenContext charGenContext)
		: base(charGenContext, FeatureGroup.ChargenPsyker, CharGenPhaseType.SanctionedPsyker, (ReactiveProperty<CharGenPhaseBaseVM>)null)
	{
	}

	protected override CharGenBackgroundBaseItemVM CreateItem(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
	{
		return new CharGenSanctionedPsykerItemVM(selectionItem, selectionStateFeature, phaseType);
	}
}
