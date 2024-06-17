using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.Tooltip;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip;

public abstract class ComparativeTooltipView : ViewBase<ComparativeTooltipVM>
{
	[FormerlySerializedAs("m_TooltipPCView")]
	[SerializeField]
	private TooltipBaseView m_TooltipView;

	[SerializeField]
	private RectTransform m_TooltipContainer;

	private readonly List<TooltipBaseView> m_Widgets = new List<TooltipBaseView>();

	private CanvasGroup m_CanvasGroup;

	private Tweener m_ShowTween;

	private bool m_IsShowed;

	private bool m_IsInit;

	private CanvasGroup CanvasGroup => m_CanvasGroup = ((m_CanvasGroup != null) ? m_CanvasGroup : base.gameObject.EnsureComponent<CanvasGroup>());

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
		foreach (TooltipVM tooltipVm in base.ViewModel.TooltipVms)
		{
			TooltipBaseView widget = WidgetFactory.GetWidget(m_TooltipView);
			widget.Initialize();
			widget.transform.SetParent(m_TooltipContainer, worldPositionStays: false);
			widget.Bind(tooltipVm);
			m_Widgets.Add(widget);
		}
		base.gameObject.SetActive(value: true);
		CanvasGroup.alpha = 0f;
	}

	protected void Show(List<Vector2> forcedPivots = null)
	{
		UIUtility.SetPopupWindowPosition((RectTransform)base.transform, base.ViewModel.TooltipVms.FirstOrDefault()?.OwnerTransform, forcedPivots);
		m_ShowTween = CanvasGroup.DOFade(1f, 0.2f).OnComplete(delegate
		{
			UISounds.Instance.Sounds.Tooltip.TooltipShow.Play();
			m_IsShowed = true;
		}).SetUpdate(isIndependentUpdate: true);
	}

	protected override void DestroyViewImplementation()
	{
		if (m_IsShowed)
		{
			UISounds.Instance.Sounds.Tooltip.TooltipHide.Play();
		}
		m_IsShowed = false;
		m_Widgets.ForEach(WidgetFactory.DisposeWidget);
		CanvasGroup.alpha = 0f;
		base.gameObject.SetActive(value: false);
		m_ShowTween?.Kill();
		m_ShowTween = null;
	}
}
