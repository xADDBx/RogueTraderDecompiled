using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem.Core;
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
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;

public class RankEntryUltimateFeatureUpgradeItemCommonView : VirtualListElementViewBase<BaseRankEntryFeatureVM>, IWidgetView, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IHasTooltipTemplate
{
	[SerializeField]
	private TextMeshProUGUI m_Description;

	[SerializeField]
	private OwlcatMultiButton m_MainButton;

	[SerializeField]
	private GameObject m_FocusedMark;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		m_Description.text = base.ViewModel.FactDescription;
		AddDisposable(m_MainButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			DoClick();
		}));
		AddDisposable(m_MainButton.OnConfirmClickAsObservable().Subscribe(delegate
		{
			DoClick();
		}));
		AddDisposable(m_MainButton.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig
		{
			PriorityPivots = new List<Vector2>
			{
				new Vector2(1f, 0.5f)
			}
		}));
		AddDisposable(base.ViewModel.FeatureState.Subscribe(delegate(RankFeatureState value)
		{
			m_MainButton.SetActiveLayer(value.ToString());
		}));
		AddDisposable(base.ViewModel.FocusedState.Subscribe(delegate(bool value)
		{
			bool active = value && base.ViewModel.FeatureState.Value != RankFeatureState.Committed && base.ViewModel.FeatureState.Value != RankFeatureState.Selected;
			m_FocusedMark.Or(null)?.SetActive(active);
		}));
	}

	protected override void DestroyViewImplementation()
	{
		base.ViewModel.SetFocusOn(null);
	}

	private void DoClick()
	{
		RankFeatureState value = base.ViewModel.FeatureState.Value;
		if (value != RankFeatureState.NotActive && value != RankFeatureState.NotValid)
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
	}

	public bool IsValid()
	{
		return true;
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.Tooltip.Value;
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
		return (base.ViewModel.FeatureState.Value == RankFeatureState.Selected) ? UIStrings.Instance.CharGen.Next : UIStrings.Instance.CommonTexts.Select;
	}
}
