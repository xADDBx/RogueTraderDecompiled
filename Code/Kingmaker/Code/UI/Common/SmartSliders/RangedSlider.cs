using System;
using DG.Tweening;
using UnityEngine;

namespace Kingmaker.Code.UI.Common.SmartSliders;

public class RangedSlider : BaseRangedSlider
{
	[SerializeField]
	private Color m_ColorStart;

	[SerializeField]
	private Color m_ColorEnd;

	[SerializeField]
	private float m_BlinkDuration = 0.4f;

	public void SetRange(float from, float to, bool blink)
	{
		float num = m_MainSlider.maxValue - m_MainSlider.minValue;
		float from2 = (from - m_MainSlider.minValue) / num;
		float to2 = (to - m_MainSlider.minValue) / num;
		SetNormalizedRange(from2, to2);
		m_Tweener?.Kill();
		if (blink && !(Math.Abs(from - to) < 0.01f))
		{
			m_Tweener = m_RangedImage.DOColor(m_ColorEnd, m_BlinkDuration).ChangeStartValue(m_ColorStart).SetLoops(-1, LoopType.Yoyo)
				.SetUpdate(isIndependentUpdate: true)
				.SetAutoKill(autoKillOnCompletion: true);
		}
	}

	public void Clear()
	{
		m_Tweener?.Kill();
		SetNormalizedRange(0f, 0f);
	}
}
