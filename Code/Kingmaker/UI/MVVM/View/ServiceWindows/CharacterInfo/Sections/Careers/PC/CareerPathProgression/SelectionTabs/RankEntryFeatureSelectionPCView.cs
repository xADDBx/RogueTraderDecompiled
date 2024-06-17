using System;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.PC.CareerPathProgression.SelectionTabs;

public class RankEntryFeatureSelectionPCView : BaseCareerPathSelectionTabPCView<RankEntrySelectionVM>
{
	[Header("UltimateFeatures")]
	[SerializeField]
	private CharInfoFeaturePCView m_UltimateFeaturePCView;

	[Header("Filters")]
	[SerializeField]
	private FeaturesFilterBaseView m_FeaturesFilter;

	[SerializeField]
	private TextMeshProUGUI m_NoFeaturesText;

	[Header("Selector")]
	[SerializeField]
	private VirtualListVertical m_VirtualList;

	[Header("Elements")]
	[SerializeField]
	private SeparatorElementView m_SeparatorElementView;

	[SerializeField]
	private RankEntryStatItemCommonView m_RankEntryStatItemCommonView;

	[SerializeField]
	private RankEntryFeatureItemCommonView m_RankEntryFeatureItemCommonView;

	[SerializeField]
	private RankEntryUltimateFeatureUpgradeItemCommonView m_RankEntryUltimateFeatureUpgradeItemCommonView;

	[SerializeField]
	private RankEntryDescriptionView m_RankEntryDescriptionView;

	private Action<bool> m_ReturnAction;

	private readonly ReactiveCollection<VirtualListElementVMBase> m_VMCollection = new ReactiveCollection<VirtualListElementVMBase>();

	public override void Initialize()
	{
		base.Initialize();
		m_VirtualList.Initialize(new VirtualListElementTemplate<RankEntrySelectionFeatureVM>(m_RankEntryFeatureItemCommonView, 0), new VirtualListElementTemplate<RankEntrySelectionFeatureVM>(m_RankEntryUltimateFeatureUpgradeItemCommonView, 1), new VirtualListElementTemplate<RankEntrySelectionStatVM>(m_RankEntryStatItemCommonView, 0), new VirtualListElementTemplate<SeparatorElementVM>(m_SeparatorElementView), new VirtualListElementTemplate<RankEntryDescriptionVM>(m_RankEntryDescriptionView));
		m_FeaturesFilter.Or(null)?.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_UltimateFeaturePCView.Bind(base.ViewModel.UltimateFeature);
		AddDisposable(base.ViewModel.EntryState.Subscribe(delegate
		{
			SetHeader(UIStrings.Instance.CharacterSheet.GetFeatureGroupHint(base.ViewModel.FeatureGroup, base.ViewModel.CanChangeSelection));
		}));
		AddDisposable(m_VirtualList.Subscribe(m_VMCollection));
		m_FeaturesFilter.Or(null)?.Bind(base.ViewModel.FeaturesFilterVM);
		AddDisposable(base.ViewModel.OnFilterChange.Subscribe(delegate
		{
			UpdateCollection();
		}));
		if (base.ViewModel.SelectedFeature.Value != null)
		{
			m_VirtualList.ScrollController.ForceScrollToElement(base.ViewModel.SelectedFeature.Value);
		}
		AddDisposable(base.ViewModel.CareerPathVM.CanCommit.Subscribe(delegate(bool canCommit)
		{
			bool flag = canCommit && base.ViewModel.CareerPathVM.LastEntryToUpgrade == base.ViewModel;
			SetNextButtonLabel(flag ? UIStrings.Instance.CharacterSheet.ToSummaryTab : UIStrings.Instance.CharGen.Next);
			SetBackButtonLabel(UIStrings.Instance.CharGen.Back);
			SetButtonSound(flag ? UISounds.ButtonSoundsEnum.NormalSound : UISounds.ButtonSoundsEnum.DoctrineNextSound);
		}));
		AddDisposable(base.ViewModel.CareerPathVM.ReadOnly.Subscribe(delegate(bool ro)
		{
			SetButtonVisibility(!ro);
		}));
		SetupNoFeaturesText();
		UpdateCollection();
		TextHelper.AppendTexts(m_NoFeaturesText);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		if (base.ViewModel.CareerPathVM.CanCommit.Value)
		{
			SetButtonVisibility(value: false);
		}
		else
		{
			UpdateState();
		}
		m_VMCollection.Clear();
		m_FeaturesFilter.Or(null)?.Unbind();
	}

	private void UpdateCollection()
	{
		m_VMCollection.Clear();
		foreach (VirtualListElementVMBase filteredGroup in base.ViewModel.FilteredGroupList)
		{
			m_VMCollection.Add(filteredGroup);
		}
		m_NoFeaturesText.gameObject.SetActive(!base.ViewModel.FilteredGroupList.Any());
	}

	public override void UpdateState()
	{
		SetButtonInteractable(base.ViewModel.SelectionMadeAndValid);
		SetFirstSelectableVisibility(base.ViewModel.CareerPathVM.FirstSelectable != null);
		SetFirstSelectableInteractable(base.ViewModel.CareerPathVM.HasDifferentFirstSelectable);
		HintText.Value = GetHintText();
	}

	private string GetHintText()
	{
		if (base.ViewModel.SelectionMadeAndValid)
		{
			return string.Empty;
		}
		return UIStrings.Instance.CharacterSheet.SelectFeatureButtonHint.Text;
	}

	protected override void HandleClickNext()
	{
		if (base.ViewModel.CareerPathVM.CanCommit.Value && base.ViewModel.CareerPathVM.LastEntryToUpgrade == base.ViewModel)
		{
			base.ViewModel.CareerPathVM.SetRankEntry(null);
		}
		else
		{
			base.ViewModel.CareerPathVM.SelectNextItem();
		}
	}

	protected override void HandleClickBack()
	{
		base.ViewModel.CareerPathVM.SelectPreviousItem();
	}

	protected override void HandleFirstSelectableClick()
	{
		base.ViewModel.CareerPathVM.SetFirstSelectableRankEntry();
	}

	private void SetupNoFeaturesText()
	{
		if (base.ViewModel.HasFeatures)
		{
			m_NoFeaturesText.text = UIStrings.Instance.CharacterSheet.NoFeaturesInFilter;
			return;
		}
		string text = base.ViewModel.FeatureGroup switch
		{
			FeatureGroup.FirstCareerTalent => UIStrings.Instance.CharacterSheet.AscensionFirstCareerTalentFeatureGroupDescription, 
			FeatureGroup.FirstCareerAbility => UIStrings.Instance.CharacterSheet.AscensionFirstCareerAbilityFeatureGroupDescription, 
			FeatureGroup.SecondCareerTalent => UIStrings.Instance.CharacterSheet.AscensionSecondCareerTalentFeatureGroupDescription, 
			FeatureGroup.SecondCareerAbility => UIStrings.Instance.CharacterSheet.AscensionSecondCareerAbilityFeatureGroupDescription, 
			FeatureGroup.FirstOrSecondCareerTalent => UIStrings.Instance.CharacterSheet.AscensionFirstOrSecondCareerTalentFeatureGroupDescription, 
			FeatureGroup.FirstOrSecondCareerAbility => UIStrings.Instance.CharacterSheet.AscensionFirstOrSecondCareerAbilityFeatureGroupDescription, 
			_ => UIStrings.Instance.CharacterSheet.NoFeaturesInFilter, 
		};
		m_NoFeaturesText.text = text;
	}
}
