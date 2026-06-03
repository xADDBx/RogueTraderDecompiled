using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.TurnTimer;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.TurnTimer;

public class TurnTimerView : ViewBase<TurnTimerVM>
{
	[SerializeField]
	protected RectTransform m_ContainterTransform;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	protected CanvasGroup m_FiveSecsFadeContainer;

	[SerializeField]
	private GameObject m_SliderContainer;

	[SerializeField]
	private Image m_SliderImage;

	[SerializeField]
	private GameObject m_GreenBackground;

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: false);
		AddDisposable(base.ViewModel.Counter.Subscribe(delegate(string text)
		{
			m_Label.text = string.Format(UIStrings.Instance.UITimer.FormattingTimerLabel, text);
		}));
		AddDisposable(base.ViewModel.Progress.Subscribe(delegate(float progress)
		{
			m_SliderImage.fillAmount = progress;
		}));
		m_SliderContainer.SetActive(value: true);
		AddDisposable(base.ViewModel.IsShowing.Subscribe(delegate(bool isShowing)
		{
			if (isShowing)
			{
				OnShow();
			}
			else
			{
				OnHide();
			}
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}

	protected virtual void OnShow()
	{
		UISounds.Instance.Sounds.EtudeCounterSound.EtudeCounterShow.Play();
	}

	protected virtual void OnHide()
	{
		UISounds.Instance.Sounds.EtudeCounterSound.EtudeCounterHide.Play();
	}

	protected void TweenTimeoutReset()
	{
		DelayedInvoker.InvokeInTime(ResetScaleInternal, 2f);
	}

	private void ResetScaleInternal()
	{
		if (!Mathf.Approximately(m_ContainterTransform.gameObject.transform.localScale.x, 1f))
		{
			m_ContainterTransform.DOComplete();
			m_ContainterTransform.DOKill();
			m_ContainterTransform.gameObject.transform.localScale = Vector3.one * 1f;
		}
	}
}
