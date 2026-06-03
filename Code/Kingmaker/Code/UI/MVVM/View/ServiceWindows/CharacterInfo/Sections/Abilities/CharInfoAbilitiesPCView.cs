using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.UI.Controls.Other;
using UniRx;
using UniRx.Triggers;
using Warhammer.SpaceCombat.StarshipLogic;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;

public class CharInfoAbilitiesPCView : CharInfoAbilitiesBaseView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if ((m_GroupByButtonsObject != null && base.ViewModel.Unit.Value.IsStarship()) || base.ViewModel.Unit.Value.IsPlayerShip() || (m_GroupByButtonsObject != null && CurrentSelectedTab.Value == CurrentInfoAbilitiesTab.Augmentations))
		{
			m_GroupByButtonsObject.SetActive(value: false);
		}
		else
		{
			m_GroupByButtonsObject.SetActive(value: true);
		}
		AddDisposable(m_GroupByTypeButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SetGroupingMode(FeatureGroupingMode.ByType);
		}));
		AddDisposable(m_GroupBySourceButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.SetGroupingMode(FeatureGroupingMode.BySource);
		}));
		AddDisposable(m_ActiveAbilities.OnPointerClickAsObservable().Subscribe(delegate
		{
			SetActiveAbilitiesState(CurrentInfoAbilitiesTab.ActiveAbilities);
		}));
		AddDisposable(m_PassiveAbilities.OnPointerClickAsObservable().Subscribe(delegate
		{
			SetActiveAbilitiesState(CurrentInfoAbilitiesTab.PassiveAbilities);
		}));
		AddDisposable(m_Augmentations.OnPointerClickAsObservable().Subscribe(delegate
		{
			SetActiveAbilitiesState(CurrentInfoAbilitiesTab.Augmentations);
		}));
	}
}
