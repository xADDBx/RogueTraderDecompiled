using System;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.View.TurnTimer;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker;

public class TurnTimerPCView : TurnTimerView
{
	private const float HideBackstopGrace = 0.1f;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	private IDisposable m_HideBackstop;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.IsFiveSecsLeft.Subscribe(delegate(bool value)
		{
			if (value)
			{
				m_FiveSecsFadeContainer.DOFade(0f, 1f).SetEase(Ease.Linear).SetLoops(5, LoopType.Yoyo);
			}
		}));
	}

	protected override void OnShow()
	{
		base.OnShow();
		m_HideBackstop?.Dispose();
		m_HideBackstop = null;
		m_FiveSecsFadeContainer.DOKill();
		m_FiveSecsFadeContainer.alpha = 1f;
		base.gameObject.SetActive(value: true);
		m_ContainterTransform.DOComplete();
		m_ContainterTransform.DOKill();
		m_FadeAnimator.AppearAnimation();
		m_ContainterTransform.DOAnchorPosY(0f, 0.5f).From(isRelative: true).SetDelay(1f)
			.SetEase(Ease.InExpo);
		m_ContainterTransform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f).From(isRelative: true).SetEase(Ease.InExpo);
		TweenTimeoutReset();
	}

	protected override void OnHide()
	{
		base.OnHide();
		m_FiveSecsFadeContainer.DOComplete();
		m_FiveSecsFadeContainer.DOKill();
		m_ContainterTransform.DOComplete();
		m_ContainterTransform.DOKill();
		m_FadeAnimator.DisappearAnimation(delegate
		{
			base.gameObject.SetActive(value: false);
		});
		m_HideBackstop?.Dispose();
		m_HideBackstop = DelayedInvoker.InvokeInTime(delegate
		{
			if (!base.ViewModel.IsShowing.Value)
			{
				base.gameObject.SetActive(value: false);
			}
		}, m_FadeAnimator.DisappearAnimationTime + 0.1f);
		TweenTimeoutReset();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_HideBackstop?.Dispose();
		m_HideBackstop = null;
	}
}
