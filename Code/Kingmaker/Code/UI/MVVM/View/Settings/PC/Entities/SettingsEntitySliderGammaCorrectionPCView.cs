using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities;

public class SettingsEntitySliderGammaCorrectionPCView : SettingsEntitySliderPCView
{
	[SerializeField]
	private OwlcatButton m_ResetButton;

	private const float NewMin = -100f;

	private const float NewMax = 100f;

	private float OldMin => base.ViewModel.MinValue;

	private float OldMax => base.ViewModel.MaxValue;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_ResetButton.OnLeftClick.AsObservable().Subscribe(base.ViewModel.ResetToDefault));
	}

	public override void OnModificationChanged(string reason, bool allowed = true)
	{
	}

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
