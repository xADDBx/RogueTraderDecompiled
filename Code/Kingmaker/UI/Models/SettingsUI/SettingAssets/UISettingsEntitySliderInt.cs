using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.UI.Models.SettingsUI.SettingAssets;

[CreateAssetMenu(menuName = "Settings UI/Slider Int")]
public class UISettingsEntitySliderInt : UISettingsEntitySlider<int>
{
	public bool UseCustomStep;

	[ShowIf("UseCustomStep")]
	public int CustomStep;

	public override bool IsInt => true;

	public override float Step
	{
		get
		{
			if (!UseCustomStep)
			{
				return 1f;
			}
			return CustomStep;
		}
	}

	public override int DecimalPlaces => 0;

	public override float GetFloatTempValue()
	{
		int tempValue = GetTempValue();
		if ((float)tempValue < base.MinValue || (float)tempValue > base.MaxValue)
		{
			Debug.LogError($"{tempValue} in {base.name} is out of Range!");
		}
		return tempValue;
	}

	public override void SetFloatTempValue(float value)
	{
		if (value < base.MinValue || value > base.MaxValue)
		{
			Debug.LogError($"{value} in {base.name} is out of Range!");
		}
		SetTempValue((int)value);
	}
}
