using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ActionBar;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.UI.Common;
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
	private GameObject m_NoAbilitiesContainer;

	[SerializeField]
	private TextMeshProUGUI m_NoAbilitiesLabel;

	protected readonly BoolReactiveProperty ActiveAbilitiesSelected = new BoolReactiveProperty(initialValue: true);

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
		m_WidgetList.DrawEntries(null, m_WidgetAbilitiesView, strictMatching: true);
		AddDisposable(ActiveAbilitiesSelected.Subscribe(delegate
		{
			UpdateAbilitiesSelectableView();
		}));
		m_TextHelper.UpdateTextSize();
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
		DrawEntities();
		UpdateNoAbilitiesContainerView();
		m_ScrollRect.ScrollToTop();
	}

	private void DrawEntities()
	{
		AutoDisposingList<CharInfoFeatureGroupVM> vmCollection = (ActiveAbilitiesSelected.Value ? base.ViewModel.ActiveAbilities : base.ViewModel.PassiveAbilities);
		m_WidgetList.Entries?.ForEach(delegate(IWidgetView e)
		{
			e.MonoBehaviour.gameObject.SetActive(value: false);
		});
		AddDisposable(m_WidgetList.DrawMultiEntries(vmCollection, new List<CharInfoFeatureGroupPCView> { m_WidgetAbilitiesView, m_WidgetTalentsView }));
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

	protected void SetActiveAbilitiesState(bool state)
	{
		ActiveAbilitiesSelected.Value = state;
		RefreshView();
	}

	private void SetLocalizedTexts()
	{
		UITextCharSheet characterSheet = UIStrings.Instance.CharacterSheet;
		m_ActiveAbilitiesLabel.text = characterSheet.ActiveAbilitiesLabel;
		m_PassiveAbilitiesLabel.text = characterSheet.PassiveAbilitiesLabel;
		m_NoAbilitiesLabel.text = characterSheet.NoAbilitiesLabel;
		if (m_ActionBarLabel != null)
		{
			m_ActionBarLabel.text = characterSheet.ActionPanelLabel;
		}
	}

	private void UpdateNoAbilitiesContainerView()
	{
		m_NoAbilitiesContainer.SetActive(m_WidgetList.Entries?.All((IWidgetView e) => ((CharInfoFeatureGroupPCView)e).IsEmpty) ?? true);
	}
}
