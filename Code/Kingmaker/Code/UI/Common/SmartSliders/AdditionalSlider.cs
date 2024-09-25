using System;
using UniRx;

namespace Kingmaker.Code.UI.Common.SmartSliders;

public class AdditionalSlider : BaseRangedSlider
{
	private float m_Addition;

	private IDisposable m_Disposable;

	public void SetAddition(float addition)
	{
		m_Addition = addition;
		if (m_Addition == 0f)
		{
			m_Disposable?.Dispose();
			SetNormalizedRange(0f, 0f);
			return;
		}
		if (m_Disposable == null)
		{
			m_Disposable = m_MainSlider.OnValueChangedAsObservable().Subscribe(delegate
			{
				OnSliderValueChanged();
			});
		}
		OnSliderValueChanged();
	}

	private void OnSliderValueChanged()
	{
		float num = m_MainSlider.maxValue - m_MainSlider.minValue;
		float from = (m_MainSlider.value - m_MainSlider.minValue) / num;
		float to = (m_MainSlider.value + m_Addition - m_MainSlider.minValue) / num;
		SetNormalizedRange(from, to);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		m_Disposable?.Dispose();
	}
}
