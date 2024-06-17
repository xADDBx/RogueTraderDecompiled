using System.Collections.Generic;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.BackgroundBase;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Doll;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.Visual.CharacterSystem;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Occupation;

public class CharGenOccupationPhaseVM : CharGenBackgroundBasePhaseVM<CharGenBackgroundBaseItemVM>
{
	private readonly Dictionary<CharGenBackgroundBaseItemVM, (int, int)> m_RampIndicesMap = new Dictionary<CharGenBackgroundBaseItemVM, (int, int)>();

	public CharGenOccupationPhaseVM(CharGenContext charGenContext)
		: base(charGenContext, FeatureGroup.ChargenOccupation, CharGenPhaseType.Occupation, (ReactiveProperty<CharGenPhaseBaseVM>)null)
	{
		OnSelectionApplied = UpdateColorsForSelected;
		AddDisposable(CharGenContext.Doll.UpdateCommand.Subscribe(delegate
		{
			DollState doll = CharGenContext.Doll;
			if (doll != null && SelectedItem.Value != null && m_RampIndicesMap.ContainsKey(SelectedItem.Value))
			{
				m_RampIndicesMap[SelectedItem.Value] = (doll.EquipmentRampIndex, doll.EquipmentRampIndexSecondary);
			}
		}));
	}

	private void UpdateColorsForSelected()
	{
		if (m_RampIndicesMap.TryGetValue(SelectedItem.Value, out var value))
		{
			CharGenContext.Doll.SetEquipColors(value.Item1, value.Item2);
			return;
		}
		CharGenUtility.GetClothesColorsProfile(CharGenContext.Doll.Clothes, out var colorPreset);
		if (!(colorPreset == null) && colorPreset.IndexPairs.Count > 0)
		{
			RampColorPreset.IndexSet indexSet = colorPreset.IndexPairs[0];
			CharGenContext.Doll.SetEquipColors(indexSet.PrimaryIndex, indexSet.SecondaryIndex);
			m_RampIndicesMap.Add(SelectedItem.Value, (indexSet.PrimaryIndex, indexSet.SecondaryIndex));
		}
	}

	protected override CharGenBackgroundBaseItemVM CreateItem(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
	{
		return new CharGenOccupationItemVM(selectionItem, selectionStateFeature, phaseType);
	}

	public void HandleDollStateUpdated(DollState dollState)
	{
		if (m_RampIndicesMap.ContainsKey(SelectedItem.Value))
		{
			m_RampIndicesMap[SelectedItem.Value] = (dollState.EquipmentRampIndex, dollState.EquipmentRampIndexSecondary);
		}
	}
}
