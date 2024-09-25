using Kingmaker.UI.MVVM.VM.CharGen.Phases.BackgroundBase;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Homeworld.ChildPhases;

public class CharGenForgeHomeworldChildPhaseVM : CharGenBackgroundBasePhaseVM<CharGenBackgroundBaseItemVM>
{
	public CharGenForgeHomeworldChildPhaseVM(CharGenContext charGenContext)
		: base(charGenContext, FeatureGroup.ChargenForgeWorld, CharGenPhaseType.ForgeHomeworldChild, (ReactiveProperty<CharGenPhaseBaseVM>)null)
	{
	}

	protected override CharGenBackgroundBaseItemVM CreateItem(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
	{
		return new CharGenForgeHomeworldItemVM(selectionItem, selectionStateFeature, phaseType);
	}
}
