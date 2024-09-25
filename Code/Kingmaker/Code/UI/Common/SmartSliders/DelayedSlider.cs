using System;
using DG.Tweening;
using UnityEngine;

namespace Kingmaker.Code.UI.Common.SmartSliders;

public class DelayedSlider : BaseRangedSlider
{
	[SerializeField]
	private Color m_PositiveDeltaColor = Color.green;

	[SerializeField]
	private Color m_NegativeDeltaColor = Color.red;

	[SerializeField]
	private float m_DeltaShowDelay = 2f;

	[SerializeField]
	private float m_DeltaMoveTime = 1f;

	private float m_TempValue;

	public float DeltaShowDelay => m_DeltaShowDelay;

	public float DeltaMoveTime => m_DeltaMoveTime;

	public void SetValue(float value, bool showDelta = true, bool noDelay = false)
	{
		if (!((double)Math.Abs(m_MainSlider.value - value) < 0.01))
		{
			if (!showDelta)
			{
				m_MainSlider.value = value;
				SetNormalizedRange(0f, 0f);
			}
			else
			{
				SetValueInternal(value, noDelay);
			}
		}
	}

	private void SetValueInternal(float value, bool noDelay)
	{
		m_TempValue = m_MainSlider.normalizedValue;
		m_MainSlider.value = value;
		float newValue = m_MainSlider.normalizedValue;
		m_RangedImage.color = ((newValue > m_TempValue) ? m_PositiveDeltaColor : m_NegativeDeltaColor);
		m_Tweener?.Kill();
		m_Tweener = DOTween.To(() => m_TempValue, delegate(float x)
		{
			m_TempValue = x;
			SetNormalizedRange(m_TempValue, newValue);
		}, newValue, m_DeltaMoveTime).ChangeStartValue(m_TempValue).SetDelay(noDelay ? 0f : m_DeltaShowDelay)
			.SetUpdate(isIndependentUpdate: true)
			.SetAutoKill(autoKillOnCompletion: true);
	}
}
