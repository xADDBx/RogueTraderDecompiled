using Kingmaker.UI.MVVM.VM.CharGen.Phases.BackgroundBase;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.SoulMark;

public class CharGenSoulMarkPhaseVM : CharGenBackgroundBasePhaseVM<CharGenBackgroundBaseItemVM>
{
	public CharGenSoulMarkPhaseVM(CharGenContext charGenContext)
		: base(charGenContext, FeatureGroup.ChargenSoulMark, CharGenPhaseType.SoulMark, (ReactiveProperty<CharGenPhaseBaseVM>)null)
	{
	}

	protected override CharGenBackgroundBaseItemVM CreateItem(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
	{
		return new CharGenSoulMarkItemVM(selectionItem, selectionStateFeature, phaseType);
	}
}
