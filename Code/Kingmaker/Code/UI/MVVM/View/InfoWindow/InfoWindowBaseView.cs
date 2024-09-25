using System;
using Kingmaker.Code.UI.MVVM.VM.InfoWindow;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.UniRx;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.InfoWindow;

public abstract class InfoWindowBaseView : InfoBaseView<InfoWindowVM>
{
	[Header("Window")]
	[SerializeField]
	protected FadeAnimator m_Animator;

	[SerializeField]
	protected ScrollRectExtended m_ScrollRect;

	[SerializeField]
	protected bool m_IsBodyViewportMinHeight;

	[ShowIf("m_IsBodyViewportMinHeight")]
	[SerializeField]
	protected float m_MaxViewportHeight = 494f;

	[SerializeField]
	protected LayoutElement m_BodyViewportLayoutElement;

	[SerializeField]
	protected bool m_IsStartPosition;

	[SerializeField]
	protected Vector2 m_Position;

	[SerializeField]
	protected VerticalLayoutGroup m_ContentVerticalLayoutGroup;

	private const float AlphaThreshold = 0.0001f;

	private bool m_IsInit;

	private bool m_IsShowed;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			base.gameObject.SetActive(value: false);
			m_Animator.Initialize();
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		Show();
		m_ScrollRect.ScrollToTop();
		if (m_IsBodyViewportMinHeight)
		{
			AddDisposable(ObservableExtensions.Subscribe(m_ScrollRect.content.UpdateAsObservable(), delegate
			{
				m_BodyViewportLayoutElement.minHeight = Mathf.Min(m_ScrollRect.content.rect.height, m_MaxViewportHeight);
			}));
		}
		m_IsStartPosition = base.ViewModel.IsStartPos;
		SetPosition();
		if (m_ContentVerticalLayoutGroup != null)
		{
			m_ContentVerticalLayoutGroup.spacing = base.ViewModel.ContentSpacing;
		}
		AddDisposable(base.ViewModel.ForceClose.Subscribe(Close));
	}

	protected override void DestroyViewImplementation()
	{
		if (m_IsShowed)
		{
			UISounds.Instance.Sounds.Tooltip.TooltipHide.Play();
		}
		m_IsShowed = false;
		base.DestroyViewImplementation();
		Hide();
		m_IsStartPosition = false;
	}

	private void Show()
	{
		m_Animator.AppearAnimation();
		DelayedInvoker.InvokeInTime(delegate
		{
			UISounds.Instance.Sounds.Tooltip.TooltipShow.Play();
			m_IsShowed = true;
		}, m_Animator.AppearAnimationTime);
	}

	public void Hide()
	{
		m_Animator.DisappearAnimation();
		CanvasGroup component = base.gameObject.GetComponent<CanvasGroup>();
		if (component != null && Math.Abs(component.alpha - 1f) < 0.0001f)
		{
			UISounds.Instance.Sounds.Tooltip.TooltipHide.Play();
		}
	}

	protected abstract void SetPosition();

	protected void Close()
	{
		OnClose();
	}

	protected virtual void OnClose()
	{
	}
}
