using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.Controls.Toggles;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;

public class RankEntryFeatureItemCommonView : VirtualListElementViewBase<BaseRankEntryFeatureVM>, IWidgetView, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IFuncAdditionalClickHandler, IHasNeighbours, ICareerPathItem
{
	[SerializeField]
	private CharInfoFeatureSimpleBaseView m_CharInfoRankEntryView;

	[SerializeField]
	private OwlcatMultiButton m_MainButton;

	[SerializeField]
	private bool m_IsListEntry = true;

	[SerializeField]
	[ConditionalShow("m_IsListEntry")]
	private GameObject m_FocusedMark;

	[SerializeField]
	private GameObject m_SelectedMark;

	[SerializeField]
	[ConditionalShow("m_IsListEntry")]
	private Image m_RecommendMark;

	[SerializeField]
	[ConditionalShow("m_IsListEntry")]
	private OwlcatToggle m_FavoritesToggle;

	[SerializeField]
	[ConditionalShow("m_IsListEntry")]
	private CanvasGroup m_FavoritesCanvasGroup;

	[SerializeField]
	[ConditionalShow("m_IsListEntry")]
	private OwlcatMultiSelectable m_UnitHasFeature;

	private List<IFloatConsoleNavigationEntity> m_Neighbours;

	private RectTransform m_TooltipPlace;

	private Action<RectTransform> m_EnsureVisibleAction;

	private readonly ReactiveProperty<string> m_HintText = new ReactiveProperty<string>();

	public MonoBehaviour MonoBehaviour => this;

	bool ICareerPathItem.IsSelectedForUI()
	{
		return base.ViewModel.IsCurrentRankEntryItem.Value;
	}

	public void SetViewParameters(RectTransform tooltipPlace, Action<RectTransform> ensureVisibleAction)
	{
		m_TooltipPlace = tooltipPlace;
		m_EnsureVisibleAction = ensureVisibleAction;
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.FeatureState.Subscribe(delegate
		{
			m_HintText.Value = base.ViewModel.HintText;
		}));
		m_CharInfoRankEntryView.Bind(base.ViewModel);
		if (m_IsListEntry && base.ViewModel is RankEntrySelectionFeatureVM rankEntrySelectionFeatureVM)
		{
			if ((bool)m_UnitHasFeature)
			{
				string activeLayer = (rankEntrySelectionFeatureVM.UnitHasFeature ? "HasFeature" : "Restricted");
				m_UnitHasFeature.SetActiveLayer(activeLayer);
			}
			if (m_CharInfoRankEntryView is LevelupFeaturePCView levelupFeaturePCView)
			{
				levelupFeaturePCView.SetCommonState(rankEntrySelectionFeatureVM.IsCommonFeature);
			}
		}
		if ((bool)m_RecommendMark)
		{
			m_RecommendMark.gameObject.SetActive(base.ViewModel.IsRecommended);
			AddDisposable(m_RecommendMark.SetHint(UIStrings.Instance.CharacterSheet.RecommendedByCareerPath));
		}
		if ((bool)m_FavoritesToggle)
		{
			m_FavoritesToggle.Set(base.ViewModel.IsFavorite);
			AddDisposable(m_FavoritesToggle.SetHint(UIStrings.Instance.CharacterSheet.ToggleFavorites));
			AddDisposable(m_FavoritesToggle.IsOn.Skip(1).Subscribe(base.ViewModel.SetFavoritesState));
		}
		if ((bool)m_FavoritesCanvasGroup)
		{
			m_FavoritesCanvasGroup.alpha = (base.ViewModel.HasFavorites ? 1f : 0f);
			m_FavoritesCanvasGroup.blocksRaycasts = base.ViewModel.HasFavorites;
		}
		if ((bool)m_SelectedMark)
		{
			AddDisposable(base.ViewModel.IsCurrentRankEntryItem.Subscribe(OnSelectedChanged));
		}
		AddDisposable(m_MainButton.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig
		{
			TooltipPlace = m_TooltipPlace,
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f)
			}
		}));
		AddDisposable(m_MainButton.SetHint(m_HintText));
		AddDisposable(m_MainButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			DoClick();
		}));
		AddDisposable(m_MainButton.OnConfirmClickAsObservable().Subscribe(delegate
		{
			DoClick();
		}));
		AddDisposable(base.ViewModel.FocusedState.Subscribe(delegate(bool value)
		{
			if (base.ViewModel.FeatureState.Value != RankFeatureState.Committed)
			{
				m_FocusedMark.Or(null)?.SetActive(value);
			}
		}));
		AddDisposable(base.ViewModel.FeatureState.Subscribe(delegate(RankFeatureState value)
		{
			m_MainButton.SetActiveLayer(value.ToString());
		}));
	}

	protected override void DestroyViewImplementation()
	{
		base.ViewModel.SetFocusOn(null);
	}

	public void OnSelectedChanged(bool value)
	{
		m_SelectedMark.SetActive(value);
		if (value)
		{
			m_EnsureVisibleAction?.Invoke(base.transform as RectTransform);
		}
	}

	private void DoClick()
	{
		if (!m_IsListEntry)
		{
			base.ViewModel.Select();
			return;
		}
		RankFeatureState value = base.ViewModel.FeatureState.Value;
		bool flag = value != RankFeatureState.NotActive && value != RankFeatureState.NotValid;
		if (base.ViewModel.CanSelect() && flag)
		{
			base.ViewModel.Select();
			EventBus.RaiseEvent(delegate(IRankEntryFocusHandler h)
			{
				h.SetFocusOn(base.ViewModel);
			});
		}
		else
		{
			BaseRankEntryFeatureVM focusOn = (base.ViewModel.FocusedState.Value ? null : base.ViewModel);
			EventBus.RaiseEvent(delegate(IRankEntryFocusHandler h)
			{
				h.SetFocusOn(focusOn);
			});
		}
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as RankEntrySelectionFeatureVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is RankEntrySelectionFeatureVM;
	}

	public void SetFocus(bool value)
	{
		m_MainButton.SetFocus(value);
		if (value)
		{
			m_EnsureVisibleAction?.Invoke(base.transform as RectTransform);
		}
	}

	public bool IsValid()
	{
		return true;
	}

	public Vector2 GetPosition()
	{
		return base.transform.position;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return m_Neighbours;
	}

	public void SetNeighbours(List<IFloatConsoleNavigationEntity> entities)
	{
		m_Neighbours = entities;
	}

	public bool CanConfirmClick()
	{
		if (m_MainButton.CanConfirmClick())
		{
			if (m_IsListEntry)
			{
				RankFeatureState value = base.ViewModel.FeatureState.Value;
				return value == RankFeatureState.Selectable || value == RankFeatureState.Selected;
			}
			return true;
		}
		return false;
	}

	public void OnConfirmClick()
	{
		BaseUnitProgressionVM unitProgressionVM = base.ViewModel.CareerPathVM.UnitProgressionVM;
		if (base.ViewModel.FeatureState.Value != RankFeatureState.Selected || (!m_IsListEntry && unitProgressionVM.CurrentRankEntryItem.Value != base.ViewModel) || unitProgressionVM is UnitProgressionVM { IsCharGen: not false })
		{
			m_MainButton.OnConfirmClick();
			EventBus.RaiseEvent(delegate(IRankEntryConfirmClickHandler h)
			{
				h.OnRankEntryConfirmClick();
			});
		}
		else
		{
			base.ViewModel.CareerPathVM.SelectNextItem();
			UISounds.Instance.Sounds.Buttons.DoctrineNextButtonClick.Play();
		}
	}

	public string GetConfirmClickHint()
	{
		if (m_IsListEntry)
		{
			return (base.ViewModel.FeatureState.Value == RankFeatureState.Selected) ? UIStrings.Instance.CharGen.Next : UIStrings.Instance.CommonTexts.Select;
		}
		return (base.ViewModel.IsCurrentRankEntryItem.Value == (bool)this) ? UIStrings.Instance.CommonTexts.Expand : UIStrings.Instance.CommonTexts.Select;
	}

	public bool CanFuncAdditionalClick()
	{
		if (m_IsListEntry && m_FavoritesToggle.IsValid())
		{
			return base.ViewModel.HasFavorites;
		}
		return false;
	}

	public void OnFuncAdditionalClick()
	{
		m_FavoritesToggle.Set(!m_FavoritesToggle.IsOn.Value);
	}

	public string GetFuncAdditionalClickHint()
	{
		return UIStrings.Instance.CharacterSheet.ToggleFavorites;
	}
}
