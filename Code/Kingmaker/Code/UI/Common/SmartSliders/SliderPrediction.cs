using System;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.Common.SmartSliders;

public class SliderPrediction : MonoBehaviour, IDisposable
{
	[SerializeField]
	private float m_SliderAnimationTime = 0.2f;

	[SerializeField]
	private float m_SliderAnimationBlinkTime = 0.4f;

	[SerializeField]
	private Slider m_FrontSlider;

	[SerializeField]
	private Slider m_BackSlider;

	[SerializeField]
	private CanvasGroup m_BackSliderCanvasGroup;

	[SerializeField]
	private List<IDisposable> m_Disposes = new List<IDisposable>();

	private Tweener m_PredictionTweener;

	private Tweener m_PlayTweener;

	private float m_CurrentValue;

	public virtual IDisposable Bind(IReactiveProperty<float> maxValue, IReactiveProperty<float> currentValue, IReactiveProperty<float> predictionValue, bool isYellowPoints)
	{
		PlayFrontSlider((maxValue.Value > 0f) ? (currentValue.Value / maxValue.Value) : 0f, smooth: false);
		AddDisposable(currentValue.CombineLatest(maxValue, predictionValue, (float current, float max, float prediction) => new { current, max, prediction }).Subscribe(value =>
		{
			bool flag = predictionValue.Value <= currentValue.Value;
			float num = ((value.max > 0f) ? (value.current / value.max) : 0f);
			float num2 = ((value.max > 0f) ? (value.prediction / value.max) : 0f);
			m_BackSlider.value = num;
			m_PredictionTweener?.Kill();
			if (flag)
			{
				HighlightBackSlider();
			}
			else
			{
				HideBackSlider();
			}
			PlayFrontSlider(Mathf.Clamp01(flag ? num2 : num));
		}));
		return this;
	}

	private void HideBackSlider()
	{
		m_BackSliderCanvasGroup.alpha = 0f;
		m_PredictionTweener.Kill();
		m_PredictionTweener = null;
	}

	private void HighlightBackSlider()
	{
		m_PredictionTweener?.Kill();
		m_BackSliderCanvasGroup.alpha = 1f;
		m_PredictionTweener = m_BackSliderCanvasGroup.DOFade(0f, m_SliderAnimationBlinkTime).SetLoops(-1, LoopType.Yoyo).SetUpdate(isIndependentUpdate: true)
			.SetAutoKill(autoKillOnCompletion: true);
		m_PredictionTweener.Play();
	}

	private void PlayFrontSlider(float value, bool smooth = true)
	{
		if (!smooth)
		{
			m_CurrentValue = value;
			m_FrontSlider.value = value;
		}
		else
		{
			if (!(Math.Abs(m_CurrentValue - value) > Mathf.Epsilon))
			{
				return;
			}
			m_PlayTweener?.Kill();
			m_CurrentValue = value;
			if (m_SliderAnimationTime == 0f)
			{
				m_FrontSlider.value = value;
				return;
			}
			m_PlayTweener = DOTween.To(() => m_FrontSlider.value, delegate(float x)
			{
				m_FrontSlider.value = x;
			}, value, m_SliderAnimationTime).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
		}
	}

	protected void AddDisposable(IDisposable iDisposable)
	{
		m_Disposes.Add(iDisposable);
	}

	public void Dispose()
	{
		m_PlayTweener?.Kill();
		m_PlayTweener = null;
		m_PredictionTweener?.Kill();
		m_PredictionTweener = null;
		m_Disposes.ForEach(delegate(IDisposable d)
		{
			d.Dispose();
		});
		m_Disposes.Clear();
	}
}
