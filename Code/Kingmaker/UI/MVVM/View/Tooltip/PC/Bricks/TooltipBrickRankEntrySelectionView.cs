using System;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks;

public class TooltipBrickRankEntrySelectionView : TooltipBaseBrickView<TooltipBrickRankEntrySelectionVM>, IUpdateContainerElements
{
	[SerializeField]
	private CharInfoFeatureSimpleBaseView m_CharInfoRankEntryView;

	[SerializeField]
	protected OwlcatMultiButton m_MainButton;

	[SerializeField]
	private Image m_MainButtonImage;

	[SerializeField]
	protected LayoutElement m_IconBlockLayout;

	[SerializeField]
	private RankEntrySelectionStateSprites[] m_StateSprites;

	[SerializeField]
	private TextMeshProUGUI m_SelectionLabel;

	[SerializeField]
	private float m_DefaultHeight = 62f;

	[SerializeField]
	private TalentGroupView m_TalentGroupView;

	[SerializeField]
	private GameObject m_AttributeContainer;

	[SerializeField]
	private TextMeshProUGUI m_AttributeName;

	private AccessibilityTextHelper m_TextHelper;

	private RectTransform m_RectTransform;

	private Vector2 m_InitBlockWidth;

	private Action m_RestoreIconHeight;

	protected override void BindViewImplementation()
	{
		if ((object)m_RectTransform == null)
		{
			m_RectTransform = GetComponent<RectTransform>();
		}
		m_InitBlockWidth = new Vector2(m_IconBlockLayout.minWidth, m_IconBlockLayout.preferredWidth);
		m_TextHelper = new AccessibilityTextHelper(m_SelectionLabel);
		m_AttributeContainer.Or(null)?.SetActive(value: false);
		m_TalentGroupView.Or(null)?.SetupView(null);
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.RankEntrySelectionVM.SelectedFeature.Subscribe(delegate(RankEntrySelectionFeatureVM featureVM)
		{
			m_CharInfoRankEntryView.Bind(featureVM);
			if (featureVM != null)
			{
				m_TalentGroupView.Or(null)?.SetupView(featureVM.Feature.TalentIconInfo);
				if (m_AttributeName != null && featureVM.Feature is BlueprintAttributeAdvancement blueprintAttributeAdvancement)
				{
					m_AttributeContainer.Or(null)?.SetActive(value: true);
					m_AttributeName.text = UIUtilityTexts.GetStatShortName(blueprintAttributeAdvancement.Stat);
				}
				else
				{
					m_AttributeContainer.Or(null)?.SetActive(value: false);
				}
			}
			IDisposable disposable2;
			if (featureVM == null)
			{
				IDisposable disposable = m_MainButton.SetTooltip(base.ViewModel.RankEntrySelectionVM.Tooltip);
				disposable2 = disposable;
			}
			else
			{
				disposable2 = m_MainButton.SetTooltip(featureVM.Tooltip);
			}
			AddDisposable(disposable2);
			string text = ((featureVM != null) ? featureVM.DisplayName : UIUtility.GetGlossaryEntryName(base.ViewModel.RankEntrySelectionVM.GlossaryEntryKey));
			if (featureVM is RankEntrySelectionStatVM rankEntrySelectionStatVM)
			{
				text = rankEntrySelectionStatVM.StatDisplayName;
			}
			m_SelectionLabel.text = text;
		}));
		AddDisposable(base.ViewModel.RankEntrySelectionVM.EntryState.Subscribe(UpdateState));
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_TextHelper.Dispose();
		m_RestoreIconHeight?.Invoke();
		m_RestoreIconHeight = null;
	}

	private void UpdateState(RankEntryState entryState)
	{
		RankEntrySelectionStateSprites rankEntrySelectionStateSprites = m_StateSprites.FirstOrDefault((RankEntrySelectionStateSprites p) => p.FeatureGroup == base.ViewModel.RankEntrySelectionVM.FeatureGroup);
		if (rankEntrySelectionStateSprites == null)
		{
			UILog.Warning($"Could not found sprites for feature group: {base.ViewModel.RankEntrySelectionVM.FeatureGroup}");
			m_MainButtonImage.gameObject.SetActive(value: false);
		}
		else if (entryState == RankEntryState.NotSelectable || entryState == RankEntryState.FirstSelectable || entryState == RankEntryState.WaitPreviousToSelect || entryState == RankEntryState.Selectable)
		{
			m_MainButtonImage.sprite = rankEntrySelectionStateSprites.Icon;
			m_MainButtonImage.gameObject.SetActive(value: true);
		}
		else
		{
			m_MainButtonImage.gameObject.SetActive(value: false);
		}
		m_MainButton.SetActiveLayer(entryState.ToString());
	}

	private void SetIconLayout(float minWidth, float preferredWidth)
	{
		m_RestoreIconHeight = delegate
		{
			SetIconLayout(m_InitBlockWidth.x, m_InitBlockWidth.y);
		};
		m_IconBlockLayout.minWidth = minWidth;
		m_IconBlockLayout.preferredWidth = preferredWidth;
	}

	public void UpdateElements(float height)
	{
		SetIconLayout(height, height);
	}
}
