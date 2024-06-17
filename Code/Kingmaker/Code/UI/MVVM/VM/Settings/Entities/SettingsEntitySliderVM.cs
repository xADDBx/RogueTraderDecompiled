using System;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using Owlcat.Runtime.UI.VirtualListSystem;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Settings.Entities;

public class SettingsEntitySliderVM : SettingsEntityWithValueVM, IVirtualListElementIdentifier
{
	public enum EntitySliderType
	{
		UsualSliderIndex,
		GammaCorrectionSliderIndex,
		FontSizeIndex
	}

	public const int UsualSliderIndex = 0;

	public const int GammaCorrectionSliderIndex = 1;

	public const int FontSizeIndex = 2;

	private readonly EntitySliderType m_SliderType;

	private readonly IUISettingsEntitySlider m_UISettingsEntity;

	public readonly ReadOnlyReactiveProperty<float> TempFloatValue;

	public readonly float MinValue;

	public readonly float MaxValue;

	public readonly bool IsInt;

	public readonly float Step;

	public readonly bool ShowValueText;

	public readonly int DecimalPlaces;

	public readonly bool IsPercentage;

	public readonly bool ChangeDirection;

	public int VirtualListTypeId => (int)m_SliderType;

	public SettingsEntitySliderVM(IUISettingsEntitySlider uiSettingsEntity, EntitySliderType sliderType = EntitySliderType.UsualSliderIndex, bool hideMarkImage = false)
		: base(uiSettingsEntity, hideMarkImage)
	{
		m_UISettingsEntity = uiSettingsEntity;
		m_SliderType = sliderType;
		MinValue = m_UISettingsEntity.MinValue;
		MaxValue = m_UISettingsEntity.MaxValue;
		IsInt = m_UISettingsEntity.IsInt;
		Step = m_UISettingsEntity.Step;
		ShowValueText = m_UISettingsEntity.ShowValueText;
		DecimalPlaces = m_UISettingsEntity.DecimalPlaces;
		IsPercentage = m_UISettingsEntity.IsPercentage;
		ChangeDirection = m_UISettingsEntity.ChangeDirection;
		AddDisposable(TempFloatValue = Observable.FromEvent(delegate(Action<float> h)
		{
			m_UISettingsEntity.OnTempFloatValueChanged += h;
		}, delegate(Action<float> h)
		{
			m_UISettingsEntity.OnTempFloatValueChanged -= h;
		}).ToReadOnlyReactiveProperty(m_UISettingsEntity.GetFloatTempValue()));
	}

	public float GetTempValue()
	{
		return m_UISettingsEntity.GetFloatTempValue();
	}

	public void SetTempValue(float value)
	{
		if (ModificationAllowed.Value)
		{
			m_UISettingsEntity.SetFloatTempValue(value);
		}
	}

	public void SetNextValue(int steps = 1)
	{
		float a = GetTempValue() + (IsInt ? 1f : m_UISettingsEntity.Step) * (float)steps;
		a = Mathf.Min(a, MaxValue);
		SetTempValue(a);
	}

	public void SetPrevValue(int steps = 1)
	{
		float a = GetTempValue() - (IsInt ? 1f : m_UISettingsEntity.Step) * (float)steps;
		a = Mathf.Max(a, MinValue);
		SetTempValue(a);
	}
}
