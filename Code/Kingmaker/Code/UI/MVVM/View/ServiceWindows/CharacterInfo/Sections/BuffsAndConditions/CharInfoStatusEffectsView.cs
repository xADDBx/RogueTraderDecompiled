using JetBrains.Annotations;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Summary;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.BuffsAndConditions;

public class CharInfoStatusEffectsView : CharInfoComponentView<CharInfoStatusEffectsVM>
{
	[SerializeField]
	private TextMeshProUGUI m_StatusEffectsTitle;

	[Header("No Status Effects")]
	[SerializeField]
	[UsedImplicitly]
	private FadeAnimator m_NoStatusContainer;

	[SerializeField]
	private TextMeshProUGUI m_NoStatusEffectsLabel;

	[Header("Widget Collection")]
	[SerializeField]
	protected ScrollRectExtended m_Scroll;

	[SerializeField]
	protected WidgetListMVVM m_WidgetList;

	[SerializeField]
	private StatusEffectBaseView m_WidgetEntityView;

	private AccessibilityTextHelper m_TextHelper;

	private UIStrings t => UIStrings.Instance;

	public override void Initialize()
	{
		base.Initialize();
		m_TextHelper = new AccessibilityTextHelper(m_StatusEffectsTitle, m_NoStatusEffectsLabel);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetupLabels();
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_TextHelper.Dispose();
	}

	private void SetupLabels()
	{
		m_StatusEffectsTitle.text = t.CharacterSheet.StatusEffects;
		m_NoStatusEffectsLabel.text = t.CharacterSheet.NoBuffText;
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		DrawEntities();
		DrawNoBuffsLabel();
		m_Scroll.ScrollToTop();
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.BuffsGroup.FeatureList, m_WidgetEntityView, strictMatching: true);
	}

	private void DrawNoBuffsLabel()
	{
		if (base.ViewModel.NoBuffs)
		{
			m_NoStatusContainer.AppearAnimation();
		}
		else
		{
			m_NoStatusContainer.DisappearAnimation();
		}
	}
}
