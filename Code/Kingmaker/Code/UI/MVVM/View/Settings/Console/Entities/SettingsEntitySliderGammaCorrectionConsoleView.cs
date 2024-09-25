using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.Console.Entities;

public class SettingsEntitySliderGammaCorrectionConsoleView : SettingsEntitySliderConsoleView
{
	private const float NewMin = -100f;

	private const float NewMax = 100f;

	private float OldMin => base.ViewModel.MinValue;

	private float OldMax => base.ViewModel.MaxValue;

	public override void SetupSlider()
	{
		base.SetupSlider();
		if (base.ViewModel.ShowValueText)
		{
			LabelSliderValue.text = Mathf.RoundToInt(GetNewRange(base.ViewModel.GetTempValue())).ToString();
		}
	}

	public override void SetValueFromSettings(float settingValue)
	{
		base.SetValueFromSettings(settingValue);
		if (base.ViewModel.ShowValueText)
		{
			LabelSliderValue.text = Mathf.RoundToInt(GetNewRange(settingValue)).ToString();
		}
	}

	private float GetNewRange(float oldValue)
	{
		return (oldValue - OldMin) / (OldMax - OldMin) * 200f + -100f;
	}
}
