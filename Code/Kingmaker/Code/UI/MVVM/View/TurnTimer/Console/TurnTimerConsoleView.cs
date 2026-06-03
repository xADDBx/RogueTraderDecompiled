using System;
using DG.Tweening;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.TurnTimer.Console;

public class TurnTimerConsoleView : TurnTimerView
{
	private const float HideBackstopGrace = 0.1f;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private GameObject m_EtudeCounterConsoleGO;

	private bool m_HasEtude;

	private bool m_IsShowing;

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
		m_HasEtude = true;
		TryToShow();
	}

	protected override void OnHide()
	{
		base.OnHide();
		m_HasEtude = false;
		TryToHide();
	}

	public void AddInput(InputLayer inputLayer)
	{
		AddDisposable(inputLayer.AddButton(delegate
		{
			TryToShow();
		}, 12));
		AddDisposable(inputLayer.AddButton(delegate
		{
			TryToHide();
		}, 12, InputActionEventType.ButtonJustReleased));
		AddDisposable(inputLayer.AddButton(delegate
		{
			TryToHide();
		}, 12, InputActionEventType.ButtonLongPressJustReleased));
	}

	private void TryToShow()
	{
		if (m_HasEtude && !m_IsShowing)
		{
			m_IsShowing = true;
			m_HideBackstop?.Dispose();
			m_HideBackstop = null;
			m_FiveSecsFadeContainer.DOKill();
			m_FiveSecsFadeContainer.alpha = 1f;
			base.gameObject.SetActive(value: true);
			m_FadeAnimator.AppearAnimation();
			UISounds.Instance.Sounds.GreenMessageLine.GreenMessageLineShow.Play();
		}
		TweenTimeoutReset();
	}

	private void TryToHide()
	{
		if (m_IsShowing)
		{
			m_FiveSecsFadeContainer.DOComplete();
			m_FiveSecsFadeContainer.DOKill();
			m_IsShowing = false;
			m_FadeAnimator.DisappearAnimation();
			m_HideBackstop?.Dispose();
			m_HideBackstop = DelayedInvoker.InvokeInTime(delegate
			{
				if (!m_IsShowing && m_FadeAnimator != null)
				{
					m_FadeAnimator.gameObject.SetActive(value: false);
				}
			}, m_FadeAnimator.DisappearAnimationTime + 0.1f);
			UISounds.Instance.Sounds.GreenMessageLine.GreenMessageLineHide.Play();
		}
		TweenTimeoutReset();
	}

	public void SetEtudeCounterVisible(bool value)
	{
		if (m_EtudeCounterConsoleGO != null)
		{
			m_EtudeCounterConsoleGO.SetActive(value);
		}
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_HideBackstop?.Dispose();
		m_HideBackstop = null;
	}
}
