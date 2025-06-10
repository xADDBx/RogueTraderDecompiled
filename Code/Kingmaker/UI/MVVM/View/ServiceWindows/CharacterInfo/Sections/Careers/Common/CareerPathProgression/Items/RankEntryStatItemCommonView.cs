using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;

public class RankEntryStatItemCommonView : VirtualListElementViewBase<RankEntrySelectionStatVM>, IWidgetView, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler
{
	[SerializeField]
	private TextMeshProUGUI m_StatDisplayName;

	[SerializeField]
	private TextMeshProUGUI m_StatIncreaseText;

	[SerializeField]
	private Image m_RecommendMark;

	[SerializeField]
	private GameObject m_FocusedMark;

	[SerializeField]
	private GameObject m_ShortNameBlock;

	[SerializeField]
	private TextMeshProUGUI m_ShortName;

	[SerializeField]
	private OwlcatMultiButton m_MainButton;

	private AccessibilityTextHelper m_TextHelper;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_StatDisplayName, m_StatIncreaseText);
		}
		m_StatDisplayName.text = base.ViewModel.StatDisplayName;
		m_ShortName.text = base.ViewModel.ShortName;
		m_ShortNameBlock.SetActive(!string.IsNullOrEmpty(base.ViewModel.ShortName));
		if (m_RecommendMark != null)
		{
			m_RecommendMark.gameObject.SetActive(base.ViewModel.IsRecommended);
			AddDisposable(m_RecommendMark.SetHint(UIStrings.Instance.CharacterSheet.RecommendedByCareerPath));
		}
		AddDisposable(m_MainButton.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig
		{
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f)
			}
		}));
		AddDisposable(m_MainButton.OnHoverAsObservable().Subscribe(delegate
		{
			base.ViewModel.UpdateTemplate();
		}));
		AddDisposable(m_MainButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			DoClick();
		}));
		AddDisposable(m_MainButton.OnConfirmClickAsObservable().Subscribe(delegate
		{
			DoClick();
		}));
		AddDisposable(base.ViewModel.StatIncreaseLabel.Subscribe(delegate(string value)
		{
			m_StatIncreaseText.text = value;
		}));
		AddDisposable(base.ViewModel.FeatureState.Subscribe(delegate(RankFeatureState value)
		{
			m_MainButton.SetActiveLayer(value.ToString());
		}));
		AddDisposable(base.ViewModel.FocusedState.Subscribe(delegate(bool value)
		{
			bool active = value && base.ViewModel.FeatureState.Value != RankFeatureState.Selected;
			m_FocusedMark.Or(null)?.SetActive(active);
		}));
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.ViewModel.SetFocusOn(null);
		m_TextHelper.Dispose();
	}

	private void DoClick()
	{
		RankFeatureState value = base.ViewModel.FeatureState.Value;
		if (value != RankFeatureState.NotActive && value != RankFeatureState.NotValid && value != RankFeatureState.Committed)
		{
			base.ViewModel.Select();
			EventBus.RaiseEvent(delegate(IRankEntryFocusHandler h)
			{
				h.SetFocusOn(base.ViewModel);
			});
		}
		else
		{
			RankEntrySelectionStatVM focusOn = (base.ViewModel.FocusedState.Value ? null : base.ViewModel);
			EventBus.RaiseEvent(delegate(IRankEntryFocusHandler h)
			{
				h.SetFocusOn(focusOn);
			});
		}
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as RankEntrySelectionStatVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is RankEntrySelectionStatVM;
	}

	public void SetFocus(bool value)
	{
		m_MainButton.SetFocus(value);
	}

	public bool IsValid()
	{
		return true;
	}

	public bool CanConfirmClick()
	{
		if (m_MainButton.CanConfirmClick())
		{
			RankFeatureState value = base.ViewModel.FeatureState.Value;
			return value == RankFeatureState.Selectable || value == RankFeatureState.Selected;
		}
		return false;
	}

	public void OnConfirmClick()
	{
		if (base.ViewModel.FeatureState.Value != RankFeatureState.Selected)
		{
			m_MainButton.OnConfirmClick();
			return;
		}
		base.ViewModel.CareerPathVM.SelectNextItem();
		UISounds.Instance.Sounds.Buttons.DoctrineNextButtonClick.Play();
	}

	public string GetConfirmClickHint()
	{
		return (base.ViewModel.FeatureState.Value == RankFeatureState.Selected) ? UIStrings.Instance.CharGen.Next : UIStrings.Instance.CommonTexts.Select;
	}
}
