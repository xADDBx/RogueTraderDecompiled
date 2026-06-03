using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ActionBar;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;

public abstract class CharInfoAbilitiesBaseView : CharInfoComponentView<CharInfoAbilitiesVM>
{
	[SerializeField]
	protected WidgetListMVVM m_WidgetList;

	[SerializeField]
	private CharInfoFeatureGroupPCView m_WidgetAbilitiesView;

	[SerializeField]
	private CharInfoFeatureGroupPCView m_WidgetTalentsView;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[SerializeField]
	private bool m_ExpandAll;

	[Header("Action Bar")]
	[SerializeField]
	private GameObject ActionBarContainer;

	[SerializeField]
	protected SurfaceActionBarPartAbilitiesBaseView m_ActionBarPartAbilitiesView;

	[SerializeField]
	private TextMeshProUGUI m_ActionBarLabel;

	[Header("Abilities")]
	[SerializeField]
	protected OwlcatMultiButton m_ActiveAbilities;

	[SerializeField]
	private TextMeshProUGUI m_ActiveAbilitiesLabel;

	[SerializeField]
	protected OwlcatMultiButton m_PassiveAbilities;

	[SerializeField]
	private TextMeshProUGUI m_PassiveAbilitiesLabel;

	[SerializeField]
	protected OwlcatMultiButton m_Augmentations;

	[SerializeField]
	private TextMeshProUGUI m_AugmentationsLabel;

	[SerializeField]
	private GameObject m_NoAbilitiesContainer;

	[SerializeField]
	private TextMeshProUGUI m_NoAbilitiesLabel;

	[SerializeField]
	private GameObject m_AbilitiesTypeSelectorContainer;

	[SerializeField]
	private GameObject m_AbilitiesTypeSelectorPetTypeContainer;

	[SerializeField]
	private TextMeshProUGUI m_PassiveAbilitiesPetVariantLabel;

	[SerializeField]
	protected GameObject m_GroupByButtonsObject;

	[SerializeField]
	protected OwlcatMultiButton m_GroupByTypeButton;

	[SerializeField]
	private TextMeshProUGUI m_GroupByTypeButtonText;

	[SerializeField]
	protected OwlcatMultiButton m_GroupBySourceButton;

	[SerializeField]
	private TextMeshProUGUI m_GroupBySourceButtonText;

	protected readonly BoolReactiveProperty ActiveAbilitiesSelected = new BoolReactiveProperty(initialValue: true);

	protected readonly BoolReactiveProperty AugmentationsSelected = new BoolReactiveProperty(initialValue: false);

	protected readonly ReactiveProperty<CurrentInfoAbilitiesTab> CurrentSelectedTab = new ReactiveProperty<CurrentInfoAbilitiesTab>();

	private AccessibilityTextHelper m_TextHelper;

	private const string ActiveLayerState = "Active";

	private const string NormalLayerState = "Normal";

	public override void Initialize()
	{
		base.Initialize();
		m_ActionBarPartAbilitiesView.Initialize();
		m_TextHelper = new AccessibilityTextHelper(m_ActiveAbilitiesLabel, m_PassiveAbilitiesLabel, m_NoAbilitiesLabel);
		SetLocalizedTexts();
	}

	protected override void BindViewImplementation()
	{
		m_ScrollRect.ScrollToTop();
		m_ActionBarPartAbilitiesView.Bind(base.ViewModel.ActionBarPartAbilitiesVM);
		AddDisposable(CurrentSelectedTab.Subscribe(delegate(CurrentInfoAbilitiesTab value)
		{
			OnTabSelectedHandler(value);
		}));
		AddDisposable(ActiveAbilitiesSelected.Subscribe(delegate
		{
			UpdateAbilitiesSelectableView();
		}));
		m_TextHelper.UpdateTextSize();
		if (m_GroupByButtonsObject != null)
		{
			m_GroupByTypeButtonText.text = UIStrings.Instance.CharGen.OrderByType;
			m_GroupBySourceButtonText.text = UIStrings.Instance.CharGen.OrderBySource;
			AddDisposable(base.ViewModel.GroupingMode.Subscribe(delegate(FeatureGroupingMode mode)
			{
				m_GroupByTypeButton.SetActiveLayer((mode == FeatureGroupingMode.ByType) ? "On" : "Off");
				m_GroupBySourceButton.SetActiveLayer((mode == FeatureGroupingMode.BySource) ? "On" : "Off");
				DrawEntities();
				UpdateNoAbilitiesContainerView();
				m_ScrollRect.ScrollToTop();
			}));
		}
		base.BindViewImplementation();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_ActionBarPartAbilitiesView.Unbind();
		m_TextHelper.Dispose();
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		base.ViewModel.RefreshAbilitiesList();
		BaseUnitEntity value = base.ViewModel.Unit.Value;
		if (value != null && value.IsPet)
		{
			m_PassiveAbilities.SetActiveLayer("Normal");
			ActiveAbilitiesSelected.Value = false;
			m_AbilitiesTypeSelectorContainer.SetActive(value: false);
			m_AbilitiesTypeSelectorPetTypeContainer.SetActive(value: true);
			CurrentSelectedTab.Value = CurrentInfoAbilitiesTab.PassiveAbilities;
		}
		else
		{
			m_ActiveAbilities.gameObject.SetActive(value: true);
			m_PassiveAbilities.Interactable = true;
			m_AbilitiesTypeSelectorContainer.SetActive(value: true);
			m_AbilitiesTypeSelectorPetTypeContainer.SetActive(value: false);
		}
		if (m_GroupByButtonsObject != null)
		{
			m_GroupByButtonsObject.SetActive(CurrentSelectedTab.Value != CurrentInfoAbilitiesTab.Augmentations);
		}
		DrawEntities();
		UpdateNoAbilitiesContainerView();
		m_ActionBarPartAbilitiesView.UpdateGrayscale();
		m_ScrollRect.ScrollToTop();
	}

	private void DrawEntities()
	{
		AutoDisposingList<CharInfoFeatureGroupVM> source = CurrentSelectedTab.Value switch
		{
			CurrentInfoAbilitiesTab.ActiveAbilities => base.ViewModel.ActiveAbilities, 
			CurrentInfoAbilitiesTab.PassiveAbilities => base.ViewModel.PassiveAbilities, 
			CurrentInfoAbilitiesTab.Augmentations => base.ViewModel.Augmentations, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		m_WidgetList.Entries?.ForEach(delegate(IWidgetView e)
		{
			e.MonoBehaviour.gameObject.SetActive(value: false);
		});
		AddDisposable(m_WidgetList.DrawMultiEntries(source.Where((CharInfoFeatureGroupVM e) => e.FeatureList.Count > 0).ToList(), new List<CharInfoFeatureGroupPCView> { m_WidgetAbilitiesView, m_WidgetTalentsView }, strictMatching: true));
		if (m_ExpandAll)
		{
			Expand();
		}
	}

	private void Expand()
	{
		m_WidgetList.Entries.ForEach(delegate(IWidgetView e)
		{
			((CharInfoFeatureGroupPCView)e).Expand();
		});
	}

	private void UpdateAbilitiesSelectableView()
	{
		bool value = ActiveAbilitiesSelected.Value;
		m_ActiveAbilities.SetActiveLayer(value ? "Active" : "Normal");
		m_PassiveAbilities.SetActiveLayer(value ? "Normal" : "Active");
		ActionBarContainer.SetActive(value);
	}

	protected void SetActiveAbilitiesState(CurrentInfoAbilitiesTab tab)
	{
		CurrentSelectedTab.Value = tab;
		RefreshView();
	}

	private void SetLocalizedTexts()
	{
		UITextCharSheet characterSheet = UIStrings.Instance.CharacterSheet;
		m_ActiveAbilitiesLabel.text = characterSheet.ActiveAbilitiesLabel;
		m_PassiveAbilitiesLabel.text = characterSheet.PassiveAbilitiesLabel;
		m_PassiveAbilitiesPetVariantLabel.text = characterSheet.PassiveAbilitiesLabel;
		m_NoAbilitiesLabel.text = characterSheet.NoAbilitiesLabel;
		if (m_ActionBarLabel != null)
		{
			m_ActionBarLabel.text = characterSheet.ActionPanelLabel;
		}
		m_AugmentationsLabel.text = UIStrings.Instance.UIAugmentations.CharScreenAugmentationsAugmentTabLabel;
	}

	private void UpdateNoAbilitiesContainerView()
	{
		m_NoAbilitiesContainer.SetActive(m_WidgetList.Entries?.All((IWidgetView e) => ((CharInfoFeatureGroupPCView)e).IsEmpty) ?? true);
	}

	private void OnTabSelectedHandler(CurrentInfoAbilitiesTab value)
	{
		base.ViewModel.RefreshAbilitiesList();
		switch (value)
		{
		case CurrentInfoAbilitiesTab.ActiveAbilities:
			m_ActiveAbilities.SetActiveLayer("Active");
			m_PassiveAbilities.SetActiveLayer("Normal");
			m_Augmentations.SetActiveLayer("Normal");
			break;
		case CurrentInfoAbilitiesTab.PassiveAbilities:
			m_ActiveAbilities.SetActiveLayer("Normal");
			m_PassiveAbilities.SetActiveLayer("Active");
			m_Augmentations.SetActiveLayer("Normal");
			break;
		case CurrentInfoAbilitiesTab.Augmentations:
			m_ActiveAbilities.SetActiveLayer("Normal");
			m_PassiveAbilities.SetActiveLayer("Normal");
			m_Augmentations.SetActiveLayer("Active");
			break;
		default:
			throw new ArgumentOutOfRangeException("value", value, null);
		}
		RefreshView();
	}
}
