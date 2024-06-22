using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.Tooltip;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip;

public abstract class ComparativeTooltipView : ViewBase<ComparativeTooltipVM>
{
	[SerializeField]
	private TooltipBaseView m_TooltipView;

	[SerializeField]
	private RectTransform m_MainTooltipContainer;

	[SerializeField]
	private RectTransform m_ComparativeTooltipContainer;

	protected readonly List<TooltipBaseView> Widgets = new List<TooltipBaseView>();

	private CanvasGroup m_CanvasGroup;

	private Tweener m_ShowTween;

	private bool m_IsShowed;

	private bool m_IsInit;

	private CanvasGroup CanvasGroup => m_CanvasGroup ?? (m_CanvasGroup = base.gameObject.EnsureComponent<CanvasGroup>());

	public void Initialize()
	{
		if (!m_IsInit)
		{
			base.gameObject.SetActive(value: false);
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		CreateTooltip(base.ViewModel.MainTooltip, isMain: true);
		for (int i = 0; i < base.ViewModel.TooltipVms.Count - 1; i++)
		{
			TooltipVM tooltipVM = base.ViewModel.TooltipVms[i];
			CreateTooltip(tooltipVM, isMain: false);
		}
		base.gameObject.SetActive(value: true);
		CanvasGroup.alpha = 0f;
		AddDisposable(DelayedInvoker.InvokeInTime(delegate
		{
			Show();
		}, 0.2f));
	}

	private void CreateTooltip(TooltipVM tooltipVM, bool isMain)
	{
		RectTransform parent = (isMain ? m_MainTooltipContainer : GetComparativeContainer());
		TooltipBaseView widget = WidgetFactory.GetWidget(m_TooltipView);
		widget.Initialize();
		widget.transform.SetParent(parent, worldPositionStays: false);
		widget.Bind(tooltipVM);
		Widgets.Add(widget);
	}

	protected override void DestroyViewImplementation()
	{
		if (m_IsShowed)
		{
			UISounds.Instance.Sounds.Tooltip.TooltipHide.Play();
		}
		m_IsShowed = false;
		Widgets.ForEach(WidgetFactory.DisposeWidget);
		Widgets.Clear();
		CanvasGroup.alpha = 0f;
		base.gameObject.SetActive(value: false);
		m_ShowTween?.Kill();
		m_ShowTween = null;
	}

	protected void Show(List<Vector2> forcedPivots = null)
	{
		UIUtility.SetPopupWindowPosition(m_MainTooltipContainer, base.ViewModel.MainOwnerTransform, Vector2.zero, forcedPivots ?? base.ViewModel.MainTooltip.PriorityPivots);
		if ((bool)m_ComparativeTooltipContainer)
		{
			UIUtility.SetPopupWindowPosition(m_ComparativeTooltipContainer, base.ViewModel.ComparativeOwnerTransform, Vector2.zero, base.ViewModel.FirstCompareTooltip?.PriorityPivots);
		}
		m_ShowTween = CanvasGroup.DOFade(1f, 0.2f).OnComplete(delegate
		{
			UISounds.Instance.Sounds.Tooltip.TooltipShow.Play();
			m_IsShowed = true;
		}).SetUpdate(isIndependentUpdate: true);
	}

	private RectTransform GetComparativeContainer()
	{
		if (!(base.ViewModel.MainOwnerTransform == base.ViewModel.ComparativeOwnerTransform) && !(m_ComparativeTooltipContainer == null))
		{
			return m_ComparativeTooltipContainer;
		}
		return m_MainTooltipContainer;
	}
}
