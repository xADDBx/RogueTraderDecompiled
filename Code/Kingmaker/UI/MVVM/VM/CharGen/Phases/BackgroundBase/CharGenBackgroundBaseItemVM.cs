using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.BackgroundBase;

public class CharGenBackgroundBaseItemVM : SelectionGroupEntityVM
{
	private readonly FeatureSelectionItem m_SelectionItem;

	private readonly SelectionStateFeature m_SelectionStateFeature;

	public readonly CharGenPhaseType PhaseType;

	public readonly ReactiveProperty<CharGenPhaseBaseVM> CurrentPhase;

	public string DisplayName { get; protected set; }

	public BlueprintFeature Feature => m_SelectionItem.Feature;

	public CharGenBackgroundBaseItemVM(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType, ReactiveProperty<CharGenPhaseBaseVM> currentPhase = null)
		: base(allowSwitchOff: false)
	{
		CurrentPhase = currentPhase;
		PhaseType = phaseType;
		m_SelectionItem = selectionItem;
		m_SelectionStateFeature = selectionStateFeature;
		DisplayName = selectionItem.Feature.Name;
	}

	protected override void DoSelectMe()
	{
	}

	public void ApplySelection()
	{
		m_SelectionStateFeature?.ClearSelection();
		m_SelectionStateFeature?.Select(m_SelectionItem);
		EventBus.RaiseEvent(delegate(ILevelUpManagerUIHandler h)
		{
			h.HandleUISelectionChanged();
		});
	}
}
