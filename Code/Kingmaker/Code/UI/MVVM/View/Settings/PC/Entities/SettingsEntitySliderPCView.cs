using System;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DisposableExtension;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities;

public class SettingsEntitySliderPCView : SettingsEntityWithValueView<SettingsEntitySliderVM>
{
	[SerializeField]
	protected Slider Slider;

	[SerializeField]
	protected TextMeshProUGUI LabelSliderValue;

	private readonly DisposableBooleanFlag m_ChangingFromUI = new DisposableBooleanFlag();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetupSlider();
		AddDisposable(Slider.onValueChanged.AsObservable().Subscribe(SetValueFromUI));
		AddDisposable(base.ViewModel.TempFloatValue.Subscribe(SetValueFromSettings));
		SubscribeNotAllowedSelectable(Slider);
	}

	public virtual void SetupSlider()
	{
		Slider.gameObject.SetActive(value: true);
		Slider.wholeNumbers = true;
		float num = base.ViewModel.MaxValue - base.ViewModel.MinValue;
		int num2 = (base.ViewModel.IsInt ? Convert.ToInt32(Math.Round(num)) : Convert.ToInt32(Math.Round(num / base.ViewModel.Step)));
		Slider.minValue = 0f;
		Slider.maxValue = num2;
		if (base.ViewModel.ShowValueText)
		{
			LabelSliderValue.gameObject.SetActive(value: true);
			int num3 = ((!base.ViewModel.IsInt) ? base.ViewModel.DecimalPlaces : 0);
			float num4 = 0f - base.ViewModel.GetTempValue();
			int num5 = (base.ViewModel.ChangeDirection ? 1 : (-1));
			LabelSliderValue.text = (num4 * (float)num5).ToString($"F{num3}").Replace(",", ".");
			if (base.ViewModel.IsPercentage)
			{
				LabelSliderValue.text += "%";
			}
		}
		else
		{
			LabelSliderValue.gameObject.SetActive(value: false);
		}
	}

	private float GetSettingValueFromSlider(float sliderValue)
	{
		if (!base.ViewModel.IsInt)
		{
			return base.ViewModel.MinValue + sliderValue * base.ViewModel.Step;
		}
		return base.ViewModel.MinValue + sliderValue;
	}

	private float GetSliderValueFromSetting(float settingValue)
	{
		float num = settingValue - base.ViewModel.MinValue;
		return base.ViewModel.IsInt ? Convert.ToInt32(Math.Round(num)) : Convert.ToInt32(Math.Round(num / base.ViewModel.Step));
	}

	private void SetValueFromUI(float sliderValue)
	{
		UISounds.Instance.Sounds.Settings.SettingsSliderMove.Play();
		float settingValueFromSlider = GetSettingValueFromSlider(sliderValue);
		using (m_ChangingFromUI.Retain())
		{
			base.ViewModel.SetTempValue(settingValueFromSlider);
		}
	}

	public virtual void SetValueFromSettings(float settingValue)
	{
		if (base.ViewModel.ShowValueText)
		{
			int num = ((!base.ViewModel.IsInt) ? base.ViewModel.DecimalPlaces : 0);
			float num2 = 0f - settingValue;
			int num3 = (base.ViewModel.ChangeDirection ? 1 : (-1));
			LabelSliderValue.text = (num2 * (float)num3).ToString($"F{num}").Replace(",", ".");
			if (base.ViewModel.IsPercentage)
			{
				LabelSliderValue.text += "%";
			}
		}
		if (!m_ChangingFromUI)
		{
			float sliderValueFromSetting = GetSliderValueFromSetting(settingValue);
			Slider.value = sliderValueFromSetting;
		}
	}

	public override void OnModificationChanged(string reason, bool allowed = true)
	{
		base.OnModificationChanged(reason, allowed);
		Slider.interactable = allowed;
		SetNotAllowedModificationHint(Slider);
	}

	public override bool HandleLeft()
	{
		if (base.ViewModel.ModificationAllowed.Value)
		{
			Slider.value--;
		}
		return true;
	}

	public override bool HandleRight()
	{
		if (base.ViewModel.ModificationAllowed.Value)
		{
			Slider.value++;
		}
		return true;
	}
}
